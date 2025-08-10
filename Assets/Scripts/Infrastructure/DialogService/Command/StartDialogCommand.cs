using System;
using Infrastructure.Network.Abstract;
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
}
