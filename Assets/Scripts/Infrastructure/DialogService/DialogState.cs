using System;
using System.Collections.Generic;
using Infrastructure.Network;
using Infrastructure.Network.Abstract;
using MessagePack;

namespace Infrastructure.DialogService
{
    [MessagePackObject]
    public class DialogState : INetworkState
    {
        public static DialogState Default => new(Guid.Empty, 0, null);

        [Key(0)]
        public Guid DialogId { get; private set; }
        [Key(1)]
        public byte ReplicaIndex { get; private set; }
        [Key(2)]
        public HashSet<uint> ReadyPlayers { get; private set; }

        public DialogState(Guid dialogId, byte replicaIndex, HashSet<uint> readyPlayers)
        {
            DialogId = dialogId;
            ReplicaIndex = replicaIndex;
            ReadyPlayers = readyPlayers;
        }
    }
}
