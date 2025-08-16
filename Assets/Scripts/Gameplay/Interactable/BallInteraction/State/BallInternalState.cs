using Gameplay.Interactable.BallInteraction.Command;

namespace Gameplay.Interactable.BallInteraction.State
{
    public class BallInternalState
    {
        public float DistanceSinceLastKick;
        public uint LastKickerNetId;
        public CaptureCommand CaptureContext;
    }
}
