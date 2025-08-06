using System;
using Infrastructure.Network.Abstract;

namespace Gameplay.Interactable.BallInteraction
{
    public class BallStateHolder : NetworkStateHolderBase, INetworkStateHolder<BallState>
    {
        BallState INetworkStateHolder<BallState>.State => this.GetStateOrDefault<BallState>();

        public override void OnStartServer()
        {
            base.OnStartServer();

            WriteState(BallState.Default);
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
