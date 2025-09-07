using Infrastructure.Network.Abstract;
using MessagePack;

namespace Gameplay.Interactable.BoxesInteraction.State
{
    [MessagePackObject]
    public struct BoxSharedState : INetworkState
    {
        public static BoxSharedState Default => new(0);

        [Key(0)]
        public uint HolderNetId { get; }

        [IgnoreMember]
        public bool HasHolder => HolderNetId != 0;

        public BoxSharedState(uint holderNetId)
        {
            HolderNetId = holderNetId;
        }
    }
}
