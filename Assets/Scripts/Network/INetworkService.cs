using System;

namespace Network
{
    public interface INetworkService
    {
        void RegisterStateHolder<T>(INetworkStateHolder stateHolder);
        void UnregisterStateHolder<T>(INetworkStateHolder stateHolder);

        T ReadState<T>() where T : INetworkState;
        void WriteState<T>(T state) where T : INetworkState;

        void SendCommand<T>(T command) where T : INetworkCommand;

        IDisposable ObserveCommand<T>(Action<T> observer) where T : INetworkCommand;
        IDisposable ObserveState<T>(Action<T> observer) where T : INetworkState;
    }
}
