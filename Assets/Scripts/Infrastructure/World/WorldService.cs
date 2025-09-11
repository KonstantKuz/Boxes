using System;
using System.Collections.Generic;
using UnityEngine;

namespace Infrastructure.World
{
    public class WorldService : IWorldService
    {
        private readonly Dictionary<Guid, IWorldObject> objectsById = new();
        private readonly Dictionary<Guid, List<IWorldObject>> objectsByTypeId = new();

        void IWorldService.Register(IWorldObject worldObject)
        {
            if (!objectsById.TryAdd(worldObject.Id, worldObject))
            {
                Debug.LogError($"Object {((MonoBehaviour) worldObject).gameObject.name} is already registered with ID {worldObject.Id}.");
                return;
            }

            if (worldObject.TypeId != Guid.Empty)
            {
                if (!objectsByTypeId.TryGetValue(worldObject.TypeId, out List<IWorldObject> objects))
                {
                    objectsByTypeId[worldObject.TypeId] = objects = new List<IWorldObject>();
                }

                objects.Add(worldObject);
            }
        }

        bool IWorldService.TryGetById(Guid id, out IWorldObject worldObject)
        {
            return objectsById.TryGetValue(id, out worldObject);
        }

        bool IWorldService.TryGetByTypeId(Guid typeId, out IWorldObject worldObject)
        {
            worldObject = null;

            if (objectsByTypeId.TryGetValue(typeId, out List<IWorldObject> objects) && objects.Count > 0)
            {
                worldObject = objects[0];
                return true;
            }

            return false;
        }
    }
}
