using System;
using Gameplay.Interactable.Abstract;
using Gameplay.Interactable.BallInteraction.BallReaction;
using Infrastructure;
using Infrastructure.QuestService;
using Infrastructure.QuestService.Abstract;
using Infrastructure.World;
using R3;
using Reflex.Attributes;
using UnityEngine;

namespace Configuration.QuestTasks.Beginning
{
    [Serializable]
    public class LearnStatusTask : TaskBase, IDisposable
    {
        [SerializeField]
        private TaskConfig config;

        [SerializeField]
        private string triggerId;

        [SerializeField]
        private int requiredHitPoints;

        private IWorldService worldService;
        private IDisposable disposable;

        [Inject]
        private void Construct(IWorldService worldService)
        {
            this.worldService = worldService;
        }

        public override void Start()
        {
            if (!worldService.TryGetById(triggerId, out IWorldObject statusTrigger))
            {
                this.Log(LogType.Error, $"Could not find trigger with id {triggerId}");
                return;
            }

            DestructibleReactionInitiator reactionInitiator =
                statusTrigger.Value.GetComponent<DestructibleReactionInitiator>();

            IDamageable damageable = reactionInitiator;
            damageable.Initialize(requiredHitPoints);
            disposable = damageable.CurrentHitPoints.Subscribe(OnHitsDone);
            DisplayData.Value = (config.Title.GetLocalizedString(), config.Description.GetLocalizedString());
        }

        private void OnHitsDone(int currentHitPoints)
        {
            if (currentHitPoints > 0)
            {
                return;
            }

            IsDone.Value = true;
        }

        void IDisposable.Dispose()
        {
            disposable?.Dispose();
        }
    }
}
