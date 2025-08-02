using System;
using System.Collections.Generic;

namespace Infrastructure.DialogService
{
    [Serializable]
    public class DialogState
    {
        public static DialogState Default => new DialogState(Guid.Empty, 0, null);

        public Guid DialogId { get; private set; }
        public byte ReplicaIndex { get; private set; }
        public HashSet<uint> ReadyPlayers { get; private set; }

        public DialogState()
        {

        }

        public DialogState(Guid dialogId, byte replicaIndex, HashSet<uint> readyPlayers)
        {
            DialogId = dialogId;
            ReplicaIndex = replicaIndex;
            ReadyPlayers = readyPlayers;
        }
    }
}
