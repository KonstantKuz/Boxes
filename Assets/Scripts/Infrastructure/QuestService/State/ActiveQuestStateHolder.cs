using System;
using Infrastructure.Network.Abstract;

namespace Infrastructure.QuestService.State
{
    public class ActiveQuestStateHolder : NetworkStateHolderBase, INetworkStateHolder<ActiveQuestSharedState>
    {
        public override void OnStartServer()
        {
            base.OnStartServer();

            WriteState(ActiveQuestSharedState.Default);
        }

        ActiveQuestSharedState INetworkStateHolder<ActiveQuestSharedState>.GetState()
        {
            return this.GetStateOrDefault<ActiveQuestSharedState>();
        }

        void INetworkStateHolder<ActiveQuestSharedState>.WriteState(ActiveQuestSharedState state)
        {
            WriteState(state);
        }

        IDisposable INetworkStateHolder<ActiveQuestSharedState>.Subscribe(Action<ActiveQuestSharedState> callback)
        {
            return Subscribe(callback);
        }
    }
}
