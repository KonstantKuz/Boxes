using System;
using Infrastructure.Network.Abstract;

namespace Infrastructure.DialogService.State
{
    public class DialogStateHolder : NetworkStateHolderBase, INetworkStateHolder<DialogState>
    {
        public override void OnStartServer()
        {
            base.OnStartServer();

            WriteState(DialogState.Default);
        }

        DialogState INetworkStateHolder<DialogState>.GetState()
        {
            return this.GetStateOrDefault<DialogState>();
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
