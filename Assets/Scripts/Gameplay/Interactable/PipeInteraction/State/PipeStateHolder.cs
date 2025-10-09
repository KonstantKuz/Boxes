using System;
using Infrastructure.Network.Abstract;

namespace Gameplay.Interactable.PipeInteraction.State
{
    public class PipeStateHolder : NetworkStateHolderBase, INetworkStateHolder<PipeSharedState>
    {
        public override void OnStartServer()
        {
            base.OnStartServer();
            WriteState(PipeSharedState.Default);
        }

        PipeSharedState INetworkStateHolder<PipeSharedState>.GetState()
        {
            return this.GetStateOrDefault<PipeSharedState>();
        }

        void INetworkStateHolder<PipeSharedState>.WriteState(PipeSharedState state)
        {
            WriteState(state);
        }

        IDisposable INetworkStateHolder<PipeSharedState>.Subscribe(Action<PipeSharedState> callback)
        {
            return Subscribe(callback);
        }
    }
}
