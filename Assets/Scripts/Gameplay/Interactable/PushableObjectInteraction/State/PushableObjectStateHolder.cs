using System;
using Infrastructure.Network.Abstract;

namespace Gameplay.Interactable.PushableObjectInteraction.State
{
    public class PushableObjectStateHolder : NetworkStateHolderBase, INetworkStateHolder<PushableObjectSharedState>
    {
        public override void OnStartServer()
        {
            base.OnStartServer();
            WriteState(PushableObjectSharedState.Default);
        }

        PushableObjectSharedState INetworkStateHolder<PushableObjectSharedState>.GetState()
        {
            return this.GetStateOrDefault<PushableObjectSharedState>();
        }

        void INetworkStateHolder<PushableObjectSharedState>.WriteState(PushableObjectSharedState state)
        {
            WriteState(state);
        }

        IDisposable INetworkStateHolder<PushableObjectSharedState>.Subscribe(Action<PushableObjectSharedState> callback)
        {
            return Subscribe(callback);
        }
    }
}
