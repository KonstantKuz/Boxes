using System;
using Infrastructure.InputService.Abstract;
using Infrastructure.Network.Abstract;
using Infrastructure.Network.State;
using Mirror;
using R3;
using Reflex.Attributes;

namespace Gameplay.Player.Abstract
{
    public abstract class InputAwareNetworkBehaviour : NetworkBehaviour
    {
        private IInputService inputService;
        private INetworkManager networkManager;
        private IDisposable connectionStateSubscription;
        protected GameInputActions actions;

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
                connectionStateSubscription = networkManager.ConnectionState.Subscribe(UpdateActions);
            }

            OnStartClientBehaviour();
        }

        protected virtual void OnStartClientBehaviour() { }

        public override void OnStopClient()
        {
            base.OnStopClient();
            connectionStateSubscription?.Dispose();
            OnStopClientBehaviour();
        }

        protected virtual void OnStopClientBehaviour() { }

        private void UpdateActions(ConnectionState state)
        {
            if (netId == 0)
            {
                return;
            }

            GameInputActions newActions = inputService.GetInput(netId);
            if (newActions != null)
            {
                actions = newActions;
                OnActionsUpdated(actions);
            }
        }

        protected virtual void OnActionsUpdated(GameInputActions actions) { }
    }
}
