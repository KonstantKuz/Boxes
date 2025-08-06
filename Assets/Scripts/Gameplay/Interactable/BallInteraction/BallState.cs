using Infrastructure.InteractionService.Abstract;
using Infrastructure.Network.Abstract;
using MessagePack;

namespace Gameplay.Interactable.BallInteraction
{
    [MessagePackObject]
    public class BallState : InteractableState, INetworkState
    {
        public static BallState Default => new BallState(0);

        [Key(0)]
        public uint OwnerNetId { get; }

        public BallState(uint ownerNetId)
        {
            OwnerNetId = ownerNetId;
        }
    }
}
