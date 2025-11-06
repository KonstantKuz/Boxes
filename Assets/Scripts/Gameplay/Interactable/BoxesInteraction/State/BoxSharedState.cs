using Infrastructure.Network.Abstract;
using MessagePack;
using UnityEngine;

namespace Gameplay.Interactable.BoxesInteraction.State
{
    public enum BoxActionType : byte
    {
        None = 0,
        Hold = 1,
        Release = 2,
        Throw = 3
    }

    [MessagePackObject]
    public struct BoxSharedState : INetworkState
    {
        public static BoxSharedState Default => new(0, 0, 0, BoxActionType.None, Vector3.zero);

        [Key(0)]
        public uint OwnerNetId { get; }

        [Key(1)]
        public uint HolderNetId { get; }

        [Key(2)]
        public uint LastActionId { get; }

        [Key(3)]
        public BoxActionType LastActionType { get; }

        [Key(4)]
        public Vector3 ThrowVelocity { get; }

        [IgnoreMember]
        public bool HasHolder => HolderNetId != 0;

        public BoxSharedState(
            uint ownerNetId,
            uint holderNetId,
            uint lastActionId,
            BoxActionType lastActionType,
            Vector3 throwVelocity
        )
        {
            OwnerNetId = ownerNetId;
            HolderNetId = holderNetId;
            LastActionId = lastActionId;
            LastActionType = lastActionType;
            ThrowVelocity = throwVelocity;
        }
    }
}
