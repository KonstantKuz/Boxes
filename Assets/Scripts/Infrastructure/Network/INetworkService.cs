using System;

namespace Infrastructure.Network
{
    public interface INetworkService
    {
        void RegisterStateHolder<T>(INetworkStateHolder stateHolder);
        void UnregisterStateHolder<T>(INetworkStateHolder stateHolder);

        byte[] Serialize<T>(T data);

        T ReadState<T>() where T : INetworkState;
        void WriteState<T>(T data) where T : INetworkState;

        void SendCommand<T>(T command) where T : INetworkCommand;

        IDisposable ObserveCommand<T>(Action<T> observer) where T : INetworkCommand;
        IDisposable ObserveReaction<T>(Action<T> observer) where T : INetworkCommand;
        IDisposable ObserveState<T>(Action<T> observer) where T : INetworkState;
    }
}
