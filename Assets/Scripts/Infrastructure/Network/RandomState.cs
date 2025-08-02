using MessagePack;

namespace Infrastructure.Network
{
    [MessagePackObject]
    public class RandomState : INetworkState
    {
        [Key(0)]
        public int Value;
    }
}
