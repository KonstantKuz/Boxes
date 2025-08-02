using System;
using Mirror;
using R3;
using Reflex.Attributes;

namespace Network
{
    public class NetworkStateHolderBase<T> : NetworkBehaviour, INetworkStateHolder where T : INetworkState
    {
        private readonly ReactiveProperty<byte[]> _reactiveState = new();

        private INetworkService _networkService;

        [SyncVar(hook = nameof(OnStateChanged))]
        private byte[] _state;

        byte[] INetworkStateHolder.State
        {
            get => _state;
            set => _state = value;
        }

        [Inject]
        private void Construct(INetworkService networkService)
        {
            _networkService = networkService;
        }

        public override void OnStartServer()
        {
            _networkService.RegisterStateHolder<T>(this);
        }

        private void OnStateChanged(byte[] oldState, byte[] newState)
        {
            _reactiveState.Value = newState;
        }

        IDisposable INetworkStateHolder.Subscribe(Action<byte[]> callback)
        {
            return _reactiveState.Subscribe(callback);
        }
    }
}