using Infrastructure.Network.Abstract;
using MessagePack;

namespace Gameplay.Interactable.BallInteraction.Command
{
    [MessagePackObject]
    public class CaptureCommand : INetworkCommand
    {
        [Key(0)]
        public readonly uint CaptureRootNetId;

        public CaptureCommand(uint captureRootNetId)
        {
            CaptureRootNetId = captureRootNetId;
        }
    }
}
