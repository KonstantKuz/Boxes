using Infrastructure.Network.Abstract;
using MessagePack;

namespace Gameplay.Interactable.BallInteraction.State
{
    [MessagePackObject]
    public struct BallState : INetworkState
    {
        public static BallState Default => new(0, 0);

        [Key(0)]
        public uint OwnerNetId { get; }

        [Key(1)]
        public byte KicksCount { get; }

        public BallState(uint ownerNetId, byte kicksCount)
        {
            OwnerNetId = ownerNetId;
            KicksCount = kicksCount;
        }
    }
}
