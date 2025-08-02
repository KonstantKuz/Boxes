using System;

namespace Network
{
    public interface INetworkStateHolder
    {
        byte[] State { get; set; }

        IDisposable Subscribe(Action<byte[]> callback);
    }
}