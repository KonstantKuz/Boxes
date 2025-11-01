using Infrastructure.Bootstrap;
using Infrastructure.Network.Abstract;
using MessagePack;

using CompositeResolver = MessagePack.Resolvers.CompositeResolver;
using StandardResolver = MessagePack.Resolvers.StandardResolver;
using UnityResolver = MessagePack.Unity.UnityResolver;

namespace Infrastructure.Network
{
    // ReSharper disable once UnusedType.Global
    public class MessagePackNetworkSerializer : INetworkSerializer, IInitializable
    {
        void IInitializable.Initialize()
        {
            IFormatterResolver formatterResolver =
                CompositeResolver.Create(UnityResolver.Instance, StandardResolver.Instance);

            MessagePackSerializer.DefaultOptions =
                MessagePackSerializerOptions.Standard.WithResolver(formatterResolver);
        }

        byte[] INetworkSerializer.Serialize<T>(T data)
        {
            return MessagePackSerializer.Serialize(data);
        }

        T INetworkSerializer.Deserialize<T>(byte[] data)
        {
            return data.Length > 0 ? MessagePackSerializer.Deserialize<T>(data) : default;
        }

        string INetworkSerializer.ConvertToJson(byte[] data)
        {
            return MessagePackSerializer.ConvertToJson(data);
        }
    }
}
