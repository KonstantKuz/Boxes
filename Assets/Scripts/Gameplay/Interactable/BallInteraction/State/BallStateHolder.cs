using System;
using Infrastructure.Network.Abstract;

namespace Gameplay.Interactable.BallInteraction.State
{
    public class BallStateHolder : NetworkStateHolderBase, INetworkStateHolder<BallState>
    {
        public override void OnStartServer()
        {
            base.OnStartServer();

            WriteState(BallState.Default);
        }

        BallState INetworkStateHolder<BallState>.GetState()
        {
            return this.GetStateOrDefault<BallState>();
        }

        void INetworkStateHolder<BallState>.WriteState(BallState state)
        {
            WriteState(state);
        }

        IDisposable INetworkStateHolder<BallState>.Subscribe(Action<BallState> callback)
        {
            return Subscribe(callback);
        }
    }
}
