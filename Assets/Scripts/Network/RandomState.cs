using MessagePack;

namespace Network
{
    [MessagePackObject]
    public class RandomState : INetworkState
    {
        [Key(0)]
        public int Value;
    }
}
