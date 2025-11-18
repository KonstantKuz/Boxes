using System;
using Infrastructure;
using Infrastructure.Components;
using Infrastructure.QuestService.Abstract;
using Infrastructure.World;
using Reflex.Attributes;
using UnityEngine;

namespace Configuration.QuestTasks.Shared
{
    [Serializable]
    public class RaiseEvent : TaskBase
    {
        [WorldObjectId]
        [SerializeField]
        private string targetId;

        private IWorldService worldService;

        [Inject]
        private void Construct(IWorldService worldService)
        {
            this.worldService = worldService;
        }

        public override void Start()
        {
            if (!worldService.TryGetById(targetId, out IWorldObject worldObject) ||
                !worldObject.TryGetComponent(out GameObjectEvent eventObject))
            {
                this.Log(LogType.Error, $"Target object with id {targetId} was not found");
                return;
            }

            eventObject.Raise();
            IsDone.Value = true;
        }
    }
}