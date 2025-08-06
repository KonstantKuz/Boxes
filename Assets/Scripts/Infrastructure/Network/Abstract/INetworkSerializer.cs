namespace Infrastructure.Network.Abstract
{
    public interface INetworkSerializer
    {
        byte[] Serialize<T>(T data) where T : INetworkState;
        T Deserialize<T>(byte[] data) where T : INetworkState;
    }
}
