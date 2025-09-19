using System;
using Infrastructure;
using Infrastructure.QuestService.Abstract;
using Infrastructure.World;
using R3;
using Reflex.Attributes;
using UnityEngine;
using UnityEngine.AI;

namespace Configuration.QuestTasks.Shared
{
    [Serializable]
    public class NpcNavigationTask : TaskBase, IDisposable
    {
        [SerializeField]
        private string npcId;

        [SerializeField]
        private string targetId;

        [SerializeField]
        private bool isAwaitRequired;

        private IDisposable disposable;
        private IWorldService worldService;

        [Inject]
        private void Construct(IWorldService worldService)
        {
            this.worldService = worldService;
        }

        public override void Start()
        {
            if (!worldService.TryGetById(npcId, out IWorldObject npc) ||
                !npc.TryGetComponent(out NavMeshAgent npcAgent))
            {
                this.Log(LogType.Error, $"Npc not found with id {npcId}");
                return;
            }

            if (!worldService.TryGetById(targetId, out IWorldObject target) ||
                !target.TryGetComponent(out Transform targetTransform))
            {
                this.Log(LogType.Error, $"Target not found with id {targetId}");
                return;
            }

            npcAgent.SetDestination(targetTransform.position);

            if (!isAwaitRequired)
            {
                IsDone.Value = true;
            }
            else
            {
                disposable = Observable.EveryUpdate(UnityFrameProvider.Update).Subscribe(_ =>
                {
                    if (npcAgent.remainingDistance <= npcAgent.stoppingDistance)
                    {
                        IsDone.Value = true;
                        disposable.Dispose();
                    }
                });
            }
        }

        void IDisposable.Dispose()
        {
            disposable?.Dispose();
        }
    }
}
