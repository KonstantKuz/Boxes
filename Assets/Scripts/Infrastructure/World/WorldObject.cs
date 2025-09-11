using System;
using Reflex.Attributes;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Infrastructure.World
{
    public class WorldObject : MonoBehaviour, IWorldObject
    {
        [SerializeField]
        private string id;

        [SerializeField]
        private WorldObjectTypeId typeId;

        public Guid Id => Guid.Parse(id);
        public Guid TypeId => typeId?.TypeId ?? Guid.Empty;
        public GameObject Value => gameObject;

        [Inject]
        private void Construct(IWorldService worldService)
        {
            worldService.Register(this);
        }

        [Button]
        private void GenerateId()
        {
            id = Guid.NewGuid().ToString();
        }
    }
}
