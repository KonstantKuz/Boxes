using System.Collections.Generic;
using MessagePack;

namespace Infrastructure.Network
{
    [MessagePackObject]
    public class ConnectionState : INetworkState
    {
        public static readonly ConnectionState Default = new() {Players = new HashSet<uint>()};

        [Key(0)]
        public HashSet<uint> Players { get; set; }
    }
}
