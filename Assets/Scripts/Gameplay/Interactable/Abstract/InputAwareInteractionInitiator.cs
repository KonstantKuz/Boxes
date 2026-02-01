using System;
using Infrastructure.InputService.Abstract;
using Infrastructure.Network.Abstract;
using Infrastructure.Network.State;
using R3;
using Reflex.Attributes;
using UnityEngine.InputSystem;

namespace Gameplay.Interactable.Abstract
{
    public abstract class InputAwareInteractionInitiator : InteractionInitiatorBase
    {
        private IInputService inputService;
        private INetworkManager networkManager;
        private IDisposable connectionStateSubscription;
        protected GameInputActions selfInput;

        [Inject]
        private void Construct(IInputService inputService, INetworkManager networkManager)
        {
            this.inputService = inputService;
            this.networkManager = networkManager;
            OnConstruct();
        }

        protected virtual void OnConstruct() { }

        public override void OnStartClient()
        {
            base.OnStartClient();

            if (isOwned)
            {
                connectionStateSubscription = networkManager.ConnectionState.Subscribe(OnConnectionStateChanged);
                OnConnectionStateChanged(networkManager.ConnectionState.CurrentValue);
            }

            OnStartClientInitiator();
        }

        protected virtual void OnStartClientInitiator() { }

        public override void OnStopClient()
        {
            base.OnStopClient();

            connectionStateSubscription?.Dispose();
            UnsubscribeSelfInput();

            OnStopClientInitiator();
        }

        protected virtual void OnStopClientInitiator() { }

        private void OnConnectionStateChanged(ConnectionState state)
        {
            if (netId == 0)
            {
                return;
            }

            UnsubscribeSelfInput();

            GameInputActions newActions = inputService.GetInput(netId);
            if (newActions != null)
            {
                selfInput = newActions;
                SubscribeSelfInput(selfInput);
            }
        }

        private void UnsubscribeSelfInput()
        {
            if (selfInput != null)
            {
                OnUnsubscribeSelfInput(selfInput);
                selfInput = null;
            }
        }

        protected abstract void SubscribeSelfInput(GameInputActions actions);
        protected abstract void OnUnsubscribeSelfInput(GameInputActions actions);
    }
}
