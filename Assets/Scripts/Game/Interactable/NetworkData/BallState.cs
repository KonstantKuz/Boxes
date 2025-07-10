using System;
using Infrastructure.InteractionService;

namespace Game.Interactable.NetworkData
{
    [Serializable]
    public class BallState : InteractableState
    {
        public BallState()
        {

        }

        public BallState(uint ownerNetId)
        {
            OwnerNetId = ownerNetId;
        }

        public uint OwnerNetId { get; }
    }
}
