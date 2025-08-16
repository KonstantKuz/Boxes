using System;
using Gameplay.Interactable.BallInteraction.Abstract;
using Gameplay.Interactable.BallInteraction.State;
using Infrastructure.Bootstrap;
using R3;
using Reflex.Attributes;
using UnityEngine;
using UnityEngine.Events;

namespace UI.HUD
{
    public class KicksCounter : MonoBehaviour, IInitializable, IDisposable
    {
        [SerializeField]
        private UnityEvent<string> countChanged;

        private IBallInteractionMediator ballInteractionMediator;
        private IDisposable ballStateSubscription;

        [Inject]
        private void Construct(IBallInteractionMediator ballInteractionMediator)
        {
            this.ballInteractionMediator = ballInteractionMediator;
        }

        void IInitializable.Initialize()
        {
            ballStateSubscription = ballInteractionMediator?.BallState.Subscribe(UpdateCounter);
        }

        private void UpdateCounter(BallSharedState ballState)
        {
            countChanged.Invoke(ballState.KicksCount.ToString());
        }

        void IDisposable.Dispose()
        {
            ballStateSubscription?.Dispose();
        }
    }
}
