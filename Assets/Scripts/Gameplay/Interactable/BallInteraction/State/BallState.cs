using Infrastructure.Network.Abstract;
using MessagePack;

namespace Gameplay.Interactable.BallInteraction.State
{
    [MessagePackObject]
    public class BallState : INetworkState
    {
        public static BallState Default => new(0);

        [Key(0)]
        public uint OwnerNetId { get; }

        public BallState(uint ownerNetId)
        {
            OwnerNetId = ownerNetId;
        }
    }
}
