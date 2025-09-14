using UnityEngine;

namespace Infrastructure.Extensions
{
    public static class CollisionExtension
    {
        private static Collider[] hits = new Collider[20];
        private const float Skin = 0.05f;

        public static bool IsInLayerMask(this Collision collision, LayerMask mask)
        {
            return ((1 << collision.gameObject.layer) & mask) != 0;
        }

        public static bool IntersectsAny(this Collider collider, Vector3? targetPosition = null, LayerMask? mask = null)
        {
            int layerMask = mask?.value ?? ~0;
            float radius = collider.bounds.extents.magnitude;

            return Physics.CheckSphere(
                targetPosition ?? collider.transform.position,
                radius,
                layerMask,
                QueryTriggerInteraction.Ignore
            );
        }

        public static Vector3 GetPenetrationOffset(this Collider collider, Vector3? targetPosition = null, LayerMask? mask = null)
        {
            int layerMask = mask?.value ?? ~0;
            Vector3 offset = Vector3.zero;

            int size = Physics.OverlapSphereNonAlloc(
                targetPosition ?? collider.transform.position,
                collider.bounds.extents.magnitude,
                hits,
                layerMask,
                QueryTriggerInteraction.Ignore
            );

            for (int i = 0; i < size; i++)
            {
                Collider other = hits[i];

                if (other == null || other == collider || !other.enabled || other.isTrigger)
                {
                    continue;
                }

                bool hasPenetration = Physics.ComputePenetration(
                    collider,
                    targetPosition ?? collider.transform.position,
                    collider.transform.rotation,
                    other,
                    other.transform.position,
                    other.transform.rotation,
                    out Vector3 direction,
                    out float distance
                );

                if (hasPenetration)
                {
                    offset += direction * (distance + Skin);
                }
            }

            return offset;
        }

        public static Vector3 ResolvePenetration(
            Vector3 safePosition,
            Vector3 targetPosition,
            float boundsRadius,
            LayerMask? mask = null
        )
        {
            int layerMask = mask?.value ?? ~0;
            Vector3 direction = (targetPosition - safePosition).normalized;
            float distance = Vector3.Distance(safePosition, targetPosition);

            bool hasHit = Physics.Raycast(
                safePosition,
                direction,
                out RaycastHit hit,
                distance,
                layerMask,
                QueryTriggerInteraction.Ignore
            );

            if (hasHit)
            {
                return hit.point - direction * boundsRadius;
            }

            return targetPosition;
        }
    }
}
