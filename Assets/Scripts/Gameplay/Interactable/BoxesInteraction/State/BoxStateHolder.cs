using System;
using Infrastructure.Network.Abstract;

namespace Gameplay.Interactable.BoxesInteraction.State
{
    public class BoxStateHolder : NetworkStateHolderBase, INetworkStateHolder<BoxSharedState>
    {
        public override void OnStartServer()
        {
            base.OnStartServer();

            WriteState(BoxSharedState.Default);
        }

        BoxSharedState INetworkStateHolder<BoxSharedState>.GetState()
        {
            return this.GetStateOrDefault<BoxSharedState>();
        }

        void INetworkStateHolder<BoxSharedState>.WriteState(BoxSharedState state)
        {
            WriteState(state);
        }

        IDisposable INetworkStateHolder<BoxSharedState>.Subscribe(Action<BoxSharedState> callback)
        {
            return Subscribe(callback);
        }
    }
}
