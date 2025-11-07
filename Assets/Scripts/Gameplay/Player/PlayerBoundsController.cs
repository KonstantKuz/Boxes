using CMF;
using Infrastructure.CameraService;
using Reflex.Attributes;
using UnityEngine;

namespace Gameplay.Player
{
    public class PlayerBoundsController : MonoBehaviour
    {
        [SerializeField]
        private AdvancedWalkerController walkerController;

        [SerializeField]
        private new Collider collider;

        [SerializeField]
        private float pushBackForce = 10f;

        [SerializeField]
        private float positionCorrectionOffset = 0.2f;

        private ICameraService cameraService;

        [Inject]
        private void Construct(ICameraService cameraService)
        {
            this.cameraService = cameraService;
        }

        private void FixedUpdate()
        {
            if (walkerController == null || collider == null)
            {
                return;
            }

            if (!cameraService.IsVisible(collider.bounds, out Plane outOfBoundsSide))
            {
                HandleOutOfBounds(outOfBoundsSide);
            }
        }

        private void HandleOutOfBounds(Plane outOfBoundsSide)
        {
            Vector3 simplifiedNormal = GetSimplifiedNormal(outOfBoundsSide.normal);
            Vector3 currentMomentum = walkerController.GetMomentum();
            float momentumTowardsBoundary = Vector3.Dot(currentMomentum, -simplifiedNormal);

            if (momentumTowardsBoundary > 0)
            {
                Vector3 tangent = currentMomentum - Vector3.Dot(currentMomentum, simplifiedNormal) * simplifiedNormal;
                walkerController.SetMomentum(tangent * 0.8f);
            }

            float distanceToPlane = outOfBoundsSide.GetDistanceToPoint(transform.position);
            if (distanceToPlane < 0)
            {
                float pushStrength = Mathf.Min(Mathf.Abs(distanceToPlane) * pushBackForce, pushBackForce * 2f);
                Vector3 pushBack = simplifiedNormal * pushStrength;
                walkerController.AddMomentum(pushBack * Time.fixedDeltaTime);
            }
        }

        private Vector3 GetSimplifiedNormal(Vector3 normal)
        {
            if (Mathf.Abs(normal.x) > Mathf.Abs(normal.z))
            {
                return normal.x > 0 ? Vector3.right : Vector3.left;
            }
            else
            {
                return normal.z > 0 ? Vector3.forward : Vector3.back;
            }
        }
    }
}