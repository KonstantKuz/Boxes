using System;
using Infrastructure.Network.Abstract;

namespace Gameplay.Interactable.BallInteraction.State
{
    public class BallStateHolder : NetworkStateHolderBase, INetworkStateHolder<BallSharedState>
    {
        public override void OnStartServer()
        {
            base.OnStartServer();

            WriteState(BallSharedState.Default);
        }

        BallSharedState INetworkStateHolder<BallSharedState>.GetState()
        {
            return this.GetStateOrDefault<BallSharedState>();
        }

        void INetworkStateHolder<BallSharedState>.WriteState(BallSharedState state)
        {
            WriteState(state);
        }

        IDisposable INetworkStateHolder<BallSharedState>.Subscribe(Action<BallSharedState> callback)
        {
            return Subscribe(callback);
        }
    }
}
