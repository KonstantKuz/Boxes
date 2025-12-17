using System;
using Infrastructure;
using Infrastructure.QuestService.Abstract;
using Infrastructure.World;
using R3;
using R3.Triggers;
using Reflex.Attributes;
using UnityEngine;

namespace Configuration.QuestTasks.Shared
{
    [Serializable]
    public class WaitForTriggerEnter : TaskBase, IDisposable
    {
        [WorldObjectId]
        [SerializeField]
        private string triggerId;

        [WorldObjectId]
        [SerializeField]
        private string targetId;

        private IWorldService worldService;
        private IDisposable triggerSubscription;

        [Inject]
        private void Construct(IWorldService worldService)
        {
            this.worldService = worldService;
        }

        public override void Start()
        {
            if (!worldService.TryGetById(targetId, out IWorldObject target))
            {
                this.Log(LogType.Error, $"Could not find target with id {targetId}");
                return;
            }

            if (!worldService.TryGetById(triggerId, out IWorldObject trigger))
            {
                this.Log(LogType.Error, $"Could not find target with id {triggerId}");
                return;
            }

            trigger.Value.OnTriggerEnterAsObservable().Subscribe(OnTriggerEnter);
        }

        private void OnTriggerEnter(Collider collider)
        {
            if (collider.TryGetComponent(out IWorldObject worldObject) && worldObject.Id.ToString() == targetId)
            {
                IsDone.Value = true;
            }
        }


        void IDisposable.Dispose()
        {
            triggerSubscription?.Dispose();
        }
    }
}
