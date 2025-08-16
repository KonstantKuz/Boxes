namespace Infrastructure.Network.Abstract
{
    public interface INetworkSerializer
    {
        byte[] Serialize<T>(T data);
        T Deserialize<T>(byte[] data);
        string ConvertToJson(byte[] data);
    }
}
