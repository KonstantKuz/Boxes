using Infrastructure.Network;
using Infrastructure.Network.Abstract;
using MessagePack;

namespace Infrastructure.DialogService.Command
{
    [MessagePackObject]
    public class ReadyDialogCommand : INetworkCommand
    {
        [Key(0)]
        public uint InitiatorId { get; set; }
    }
}