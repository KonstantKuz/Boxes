using System;

namespace Infrastructure.Network
{
    public interface INetworkService
    {
        void RegisterStateHolder<T>(INetworkStateHolder stateHolder);
        void UnregisterStateHolder<T>(INetworkStateHolder stateHolder);

        byte[] Serialize<T>(T data);

        T ReadState<T>() where T : INetworkState;
        void WriteState(byte type, byte[] state);

        void SendCommand(byte type, byte[] command);

        IDisposable ObserveCommand<T>(Action<T> observer) where T : INetworkCommand;
        IDisposable ObserveState<T>(Action<T> observer) where T : INetworkState;
    }
}
