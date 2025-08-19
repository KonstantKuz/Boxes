using DG.Tweening;
using Gameplay.Interactable.Abstract;
using Gameplay.Interactable.BallInteraction.Abstract;
using Gameplay.Interactable.BallInteraction.State;
using Mirror;
using R3;
using UnityEngine;

namespace Gameplay.Interactable.BallInteraction.BallReaction
{
    public class DestructibleReactionInitiator : NetworkBehaviour, IBallReactionInitiator, IDamageable
    {
        [SerializeField]
        private int maxHitPoints;

        [SyncVar(hook = nameof(OnHitPointsChanged))]
        private int currentHitPoints;

        private ReactiveProperty<int> currentHitPointsReactive;

        int IDamageable.MaxHitPoints => maxHitPoints;
        ReadOnlyReactiveProperty<int> IDamageable.CurrentHitPoints => currentHitPointsReactive;

        public override void OnStartServer()
        {
            ((IDamageable)this).Initialize(maxHitPoints);
        }

        void IDamageable.Initialize(int maxHitPoints)
        {
            currentHitPointsReactive = new ReactiveProperty<int>(maxHitPoints);
            this.maxHitPoints = maxHitPoints;
            currentHitPoints = maxHitPoints;
            gameObject.SetActive(true);
        }

        void IDamageable.TakeDamage(int hitPoints)
        {
            currentHitPoints -= hitPoints;
        }

        bool IBallReactionInitiator.TryExecuteReaction(Collision collisionInfo, BallSharedState sharedState)
        {
            if (sharedState.KicksCount >= 3)
            {
                ((IDamageable)this).TakeDamage(1);
            }

            return true;
        }

        private void OnHitPointsChanged(int oldValue, int newValue)
        {
            if (newValue < oldValue)
            {
                transform.DOShakePosition(0.5f);
            }

            if (newValue <= 0)
            {
                gameObject.SetActive(false);
            }

            currentHitPointsReactive.Value = newValue;
        }
    }
}
