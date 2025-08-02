using System;
using Infrastructure.InteractionService.Abstract;

namespace Gameplay.Interactable.BallInteraction
{
    [Serializable]
    public class BallState : InteractableState
    {
        public static BallState Default => new BallState(0);

        public uint OwnerNetId { get; }

        public BallState()
        {

        }

        public BallState(uint ownerNetId)
        {
            OwnerNetId = ownerNetId;
        }
    }
}
