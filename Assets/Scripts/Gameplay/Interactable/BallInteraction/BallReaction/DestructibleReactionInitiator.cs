using Gameplay.Interactable.Abstract;
using Gameplay.Interactable.BallInteraction.Abstract;
using Gameplay.Interactable.BallInteraction.State;
using Mirror;
using R3;
using UnityEngine;
using UnityEngine.Events;

namespace Gameplay.Interactable.BallInteraction.BallReaction
{
    public class DestructibleReactionInitiator : NetworkBehaviour, IBallReactionInitiator, IDamageable
    {
        [SerializeField]
        private UnityEvent OnHit;

        [SerializeField]
        private UnityEvent OnMaxHit;

        [SerializeField]
        private int maxHitPoints;

        [SerializeField]
        private bool isManualInitialization;

        [SyncVar(hook = nameof(OnHitPointsChanged))]
        private int currentHitPoints;

        private ReactiveProperty<int> currentHitPointsReactive;
        private bool isInitialized;

        int IDamageable.MaxHitPoints => maxHitPoints;
        ReadOnlyReactiveProperty<int> IDamageable.CurrentHitPoints => currentHitPointsReactive;

        public override void OnStartServer()
        {
            if (isManualInitialization)
            {
                return;
            }

            ((IDamageable)this).Initialize(maxHitPoints);
        }

        void IDamageable.Initialize(int maxHitPoints)
        {
            currentHitPointsReactive = new ReactiveProperty<int>(maxHitPoints);
            this.maxHitPoints = maxHitPoints;
            currentHitPoints = maxHitPoints;
            gameObject.SetActive(true);
            isInitialized = true;
        }

        void IDamageable.TakeDamage(int hitPoints)
        {
            if (!isInitialized)
            {
                return;
            }

            currentHitPoints -= hitPoints;
        }

        bool IBallReactionInitiator.TryExecuteReaction(Collision collisionInfo, BallSharedState sharedState)
        {
            if (!isInitialized)
            {
                return false;
            }

            if (sharedState.KicksCount >= 3)
            {
                ((IDamageable)this).TakeDamage(1);
            }

            return true;
        }

        private void OnHitPointsChanged(int oldValue, int newValue)
        {
            if (!isInitialized)
            {
                return;
            }

            currentHitPointsReactive.Value = newValue;

            if (newValue < oldValue)
            {
                OnHit?.Invoke();
            }

            if (newValue <= 0)
            {
                OnMaxHit?.Invoke();
            }
        }
    }
}
