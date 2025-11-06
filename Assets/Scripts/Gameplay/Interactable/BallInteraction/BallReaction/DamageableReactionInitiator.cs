using Gameplay.Interactable.Abstract;
using Gameplay.Interactable.BallInteraction.Abstract;
using Mirror;
using R3;
using UnityEngine;
using UnityEngine.Events;

namespace Gameplay.Interactable.BallInteraction.BallReaction
{
    public class DamageableReactionInitiator : NetworkBehaviour, IBallReactionInitiator, IDamageable
    {
        [SerializeField]
        private UnityEvent OnHit;

        [SerializeField]
        private UnityEvent OnMaxHit;

        [SerializeField]
        private int maxHitPoints;

        [SerializeField]
        private bool isManualInitialization;

        [SyncVar]
        private bool isInitialized;

        [SyncVar(hook = nameof(OnHitPointsChanged))]
        private int currentHitPoints;

        private ReactiveProperty<int> currentHitPointsReactive;

        int IDamageable.MaxHitPoints => maxHitPoints;
        ReadOnlyReactiveProperty<int> IDamageable.CurrentHitPoints => currentHitPointsReactive;

        private void Awake()
        {
            currentHitPointsReactive = new ReactiveProperty<int>();
        }

        public override void OnStartServer()
        {
            if (isManualInitialization)
            {
                return;
            }

            ((IDamageable)this).Initialize();
        }

        void IDamageable.Initialize(int initialHitPoints)
        {
            if (initialHitPoints > 0)
            {
                maxHitPoints = initialHitPoints;
            }

            currentHitPointsReactive = new ReactiveProperty<int>(maxHitPoints);
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

            CmdTakeDamage(hitPoints);
        }

        bool IBallReactionInitiator.TryExecuteReaction()
        {
            if (!isInitialized)
            {
                return false;
            }

            ((IDamageable)this).TakeDamage(1);

            return true;
        }

        [Command(requiresAuthority = false)]
        private void CmdTakeDamage(int hitPoints)
        {
            currentHitPoints -= hitPoints;
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
