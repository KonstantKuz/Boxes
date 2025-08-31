using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Infrastructure.World
{
    [CreateAssetMenu(
        fileName = nameof(WorldObjectTypeId),
        menuName = GlobalParams.Root + nameof(WorldObjectTypeId)
    )]
    public class WorldObjectTypeId : ScriptableObject
    {
        [SerializeField]
        private string typeId;

        public Guid TypeId => Guid.Parse(typeId);

        [Button]
        private void GenerateId()
        {
            typeId = Guid.NewGuid().ToString();
        }
    }
}
