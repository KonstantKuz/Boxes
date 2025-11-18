using System;
using System.Collections.Generic;
using UnityEngine;

namespace Infrastructure.World
{
    [Serializable]
    public class WorldService : IWorldService
    {
        private readonly Dictionary<Guid, IWorldObject> objectsById = new();

        void IWorldService.Register(IWorldObject worldObject)
        {
            if (!objectsById.TryAdd(worldObject.Id, worldObject))
            {
                Debug.LogError($"Object {((MonoBehaviour) worldObject).gameObject.name} is already registered with ID {worldObject.Id}.", worldObject.Value);
            }
        }

        bool IWorldService.TryGetById(Guid id, out IWorldObject worldObject)
        {
            return objectsById.TryGetValue(id, out worldObject);
        }
    }
}
