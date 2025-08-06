using Infrastructure.InteractionService.Abstract;
using MessagePack;

namespace Gameplay.Interactable.BallInteraction
{
    [MessagePackObject]
    public class CaptureInteractionContext : InteractionContext
    {
        [Key(0)]
        public readonly uint CaptureRootNetId;

        public CaptureInteractionContext(uint captureRootNetId)
        {
            CaptureRootNetId = captureRootNetId;
        }
    }
}
