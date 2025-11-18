using System;
using System.Collections.Generic;
using System.Linq;
using Gameplay.Interactable.Abstract;
using Gameplay.Interactable.BallInteraction.BallReaction;
using Infrastructure;
using Infrastructure.Components;
using Infrastructure.QuestService;
using Infrastructure.QuestService.Abstract;
using Infrastructure.World;
using R3;
using Reflex.Attributes;
using UnityEngine;

namespace Configuration.QuestTasks.Shared
{
    [Serializable]
    public class DamageablePoolTask : TaskBase, IDisposable
    {
        [SerializeField]
        private TaskDescription description;

        [WorldObjectId]
        [SerializeField]
        private List<string> triggerIds;

        [SerializeField]
        private int singleTriggerHitPoints;

        [SerializeField]
        private int requiredTotalDamage;

        private IWorldService worldService;
        private List<IDamageable> triggers;
        private IDisposable disposable;

        [Inject]
        private void Construct(IWorldService worldService)
        {
            this.worldService = worldService;

            triggers = new List<IDamageable>();
        }

        public override void Start()
        {
            foreach (string id in triggerIds)
            {
                if (!worldService.TryGetById(id, out IWorldObject trigger))
                {
                    this.Log(LogType.Error, $"Could not find trigger with id {id}");
                    return;
                }

                DamageableReactionInitiator reactionInitiator =
                    trigger.Value.GetComponent<DamageableReactionInitiator>();

                triggers.Add(reactionInitiator);

                IDamageable damageable = reactionInitiator;
                damageable.Initialize(singleTriggerHitPoints);
            }

            List<Observable<IDamageable>> observables =
                triggers.Select(item => item.CurrentHitPoints.Select(_ => item)).ToList();

            disposable = Observable.Merge(observables).Subscribe(OnAnyHitPointsChanged);
            DisplayData.Value = (description.Title.GetLocalizedString(), description.Description.GetLocalizedString());
        }

        private void OnAnyHitPointsChanged(IDamageable lastHit)
        {
            int totalMaxHitPoints = triggers.Sum(item => item.MaxHitPoints);
            int totalCurrentHitPoints = triggers.Sum(item => item.CurrentHitPoints.CurrentValue);
            int totalDamage = totalMaxHitPoints - totalCurrentHitPoints;

            if (totalDamage < requiredTotalDamage)
            {
                return;
            }

            if (((DamageableReactionInitiator) lastHit).TryGetComponent(out GameObjectEvent lastHitEvent))
            {
                lastHitEvent.Raise();
            }

            IsDone.Value = true;
        }

        void IDisposable.Dispose()
        {
            disposable?.Dispose();
        }
    }
}
