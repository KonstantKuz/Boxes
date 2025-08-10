using Infrastructure.Network.Abstract;
using MessagePack;

namespace Infrastructure.DialogService.Command
{
    [MessagePackObject]
    public class StopDialogCommand : INetworkCommand
    {
    }
}
