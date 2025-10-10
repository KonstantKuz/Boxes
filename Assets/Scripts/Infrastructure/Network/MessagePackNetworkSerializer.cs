using Infrastructure.Network.Abstract;
using MessagePack;

namespace Infrastructure.Network
{
    // ReSharper disable once UnusedType.Global
    public class MessagePackNetworkSerializer : INetworkSerializer
    {
        byte[] INetworkSerializer.Serialize<T>(T data)
        {
            return MessagePackSerializer.Serialize(data);
        }

        T INetworkSerializer.Deserialize<T>(byte[] data)
        {
            return data.Length > 0 ? MessagePackSerializer.Deserialize<T>(data) : default;
        }

        public string ConvertToJson(byte[] data)
        {
            return MessagePackSerializer.ConvertToJson(data);
        }
    }
}
