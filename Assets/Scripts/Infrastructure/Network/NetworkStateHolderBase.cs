using System;
using Mirror;
using R3;
using Reflex.Attributes;

namespace Infrastructure.Network
{
    public class NetworkStateHolderBase : NetworkBehaviour, INetworkStateHolder
    {
        private readonly ReactiveProperty<byte[]> reactiveState = new();

        protected INetworkService NetworkService;

        [SyncVar(hook = nameof(OnStateChanged))]
        private byte[] state;

        byte[] INetworkStateHolder.State => state;

        [Inject]
        private void Construct(INetworkService networkService)
        {
            NetworkService = networkService;
        }

        public override void OnStartClient()
        {
            OnStateChanged(null, state);
        }

        private void OnStateChanged(byte[] oldState, byte[] newState)
        {
            reactiveState.Value = newState;
        }

        protected void WriteState<T>(T defaultState)
        {
            ((INetworkStateHolder)this).WriteState(NetworkService.Serialize(defaultState));
        }

        void INetworkStateHolder.WriteState(byte[] state)
        {
            this.state = state;
        }

        IDisposable INetworkStateHolder.Subscribe(Action<byte[]> callback)
        {
            return reactiveState.Where(array => array != null).Subscribe(callback);
        }
    }
}
