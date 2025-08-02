using System;
using Mirror;
using R3;

namespace Infrastructure.Network
{
    public class NetworkStateHolderBase : NetworkBehaviour, INetworkStateHolder
    {
        private readonly ReactiveProperty<byte[]> _reactiveState = new();

        [SyncVar(hook = nameof(OnStateChanged))]
        private byte[] _state;

        byte[] INetworkStateHolder.State => _state;

        private void OnStateChanged(byte[] oldState, byte[] newState)
        {
            _reactiveState.Value = newState;
        }

        void INetworkStateHolder.WriteState(byte[] state)
        {
            _state = state;
        }

        IDisposable INetworkStateHolder.Subscribe(Action<byte[]> callback)
        {
            return _reactiveState.Where(array => array != null).Subscribe(callback);
        }
    }
}
