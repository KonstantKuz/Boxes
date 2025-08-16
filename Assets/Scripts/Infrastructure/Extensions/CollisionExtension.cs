using UnityEngine;

namespace Infrastructure.Extensions
{
    public static class CollisionExtension
    {
        public static bool IsInLayerMask(this Collision collision, LayerMask mask)
        {
            return ((1 << collision.gameObject.layer) & mask) != 0;
        }
    }
}
