using Infrastructure.Network.Abstract;
using MessagePack;

namespace Gameplay.Interactable.BallInteraction.State
{
    [MessagePackObject]
    public struct BallSharedState : INetworkState
    {
        public static BallSharedState Default => new(0, 0);

        [Key(0)]
        public uint HolderNetId { get; }

        [Key(1)]
        public byte KicksCount { get; }

        [IgnoreMember]
        public bool HasHolder => HolderNetId != 0;

        public BallSharedState(uint holderNetId, byte kicksCount)
        {
            HolderNetId = holderNetId;
            KicksCount = kicksCount;
        }
    }
}
