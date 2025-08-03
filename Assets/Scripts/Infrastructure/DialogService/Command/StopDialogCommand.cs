using Infrastructure.Network;
using MessagePack;

namespace Infrastructure.DialogService.Command
{
    [MessagePackObject]
    public class StopDialogCommand : INetworkCommand
    {
    }
}
