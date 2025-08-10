using System;
using Infrastructure.Network.Abstract;

namespace Infrastructure.Network.State
{
    public class ConnectionStateHolder : NetworkStateHolderBase, INetworkStateHolder<ConnectionState>
    {
        public override void OnStartServer()
        {
            base.OnStartServer();

            WriteState(ConnectionState.Default);
        }

        ConnectionState INetworkStateHolder<ConnectionState>.GetState()
        {
            return this.GetStateOrDefault<ConnectionState>();
        }

        void INetworkStateHolder<ConnectionState>.WriteState(ConnectionState state)
        {
            WriteState(state);
        }

        IDisposable INetworkStateHolder<ConnectionState>.Subscribe(Action<ConnectionState> callback)
        {
            return Subscribe(callback);
        }
    }
}
