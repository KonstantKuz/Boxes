using System;
using UnityEngine;

namespace Infrastructure.World
{
    public static class WorldObjectExtensions
    {
        public static bool TryGetComponent<T>(this IWorldObject worldObject, out T component)
        {
            return ((MonoBehaviour)worldObject).TryGetComponent(out component);
        }

        public static bool TryGetById(this IWorldService worldService, string id, out IWorldObject worldObject)
        {
            worldObject = null;
            return Guid.TryParse(id, out Guid guid) && worldService.TryGetById(guid, out worldObject);
        }
    }
}
