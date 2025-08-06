using System;
using Infrastructure.DialogService;
using Infrastructure.Network.Abstract;

namespace Configuration.State
{
    public class DialogStateHolder : NetworkStateHolderBase, INetworkStateHolder<DialogState>
    {
        DialogState INetworkStateHolder<DialogState>.State => this.GetStateOrDefault<DialogState>();

        public override void OnStartServer()
        {
            base.OnStartServer();

            WriteState(DialogState.Default);
        }

        void INetworkStateHolder<DialogState>.WriteState(DialogState state)
        {
            WriteState(state);
        }

        IDisposable INetworkStateHolder<DialogState>.Subscribe(Action<DialogState> callback)
        {
            return Subscribe(callback);
        }
    }
}
