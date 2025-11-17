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
    public class RaiseBoolEvent : TaskBase
    {
        [WorldObjectId]
        [SerializeField]
        private string targetId;

        [SerializeField]
        private bool value;

        private IWorldService worldService;

        [Inject]
        private void Construct(IWorldService worldService)
        {
            this.worldService = worldService;
        }

        public override void Start()
        {
            if (!worldService.TryGetById(targetId, out IWorldObject worldObject) ||
                !worldObject.TryGetComponent(out BoolEvent boolEvent))
            {
                this.Log(LogType.Error, $"Target object with id {targetId} was not found");
                return;
            }

            boolEvent.Raise(value);
            IsDone.Value = true;
        }
    }


    [Serializable]
    public class RaiseEvent : TaskBase
    {
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
