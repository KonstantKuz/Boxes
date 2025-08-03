using System;
using Infrastructure.Network;
using MessagePack;

namespace Infrastructure.DialogService.Command
{
    [MessagePackObject]
    public class StartDialogCommand : INetworkCommand
    {
        [Key(0)]
        public uint InitiatorId { get; set; }
        [Key(1)]
        public Guid DialogId { get; set; }
    }

    [MessagePackObject]
    public class ReadyDialogCommand : INetworkCommand
    {
        [Key(0)]
        public uint InitiatorId { get; set; }
    }
}
