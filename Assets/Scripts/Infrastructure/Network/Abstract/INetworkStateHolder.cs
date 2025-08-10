using System;

namespace Infrastructure.Network.Abstract
{
    public interface INetworkStateHolder<T> where T : INetworkState
    {
        T GetState();

        void WriteState(T state);

        IDisposable Subscribe(Action<T> callback);
    }
}
