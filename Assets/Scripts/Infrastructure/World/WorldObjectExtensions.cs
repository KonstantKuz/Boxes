using UnityEngine;

namespace Infrastructure.World
{
    public static class WorldObjectExtensions
    {
        public static bool TryGetComponent<T>(this IWorldObject worldObject, out T component)
        {
            return ((MonoBehaviour)worldObject).TryGetComponent(out component);
        }
    }
}
