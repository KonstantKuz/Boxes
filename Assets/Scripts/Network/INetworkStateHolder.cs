using System;

namespace Network
{
    public interface INetworkStateHolder
    {
        byte[] State { get; }

        void WriteState(byte[] state);

        IDisposable Subscribe(Action<byte[]> callback);
    }
}
