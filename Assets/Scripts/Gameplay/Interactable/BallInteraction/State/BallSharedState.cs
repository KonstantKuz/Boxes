using Infrastructure.Network.Abstract;
using MessagePack;
using UnityEngine;

namespace Gameplay.Interactable.BallInteraction.State
{
    public enum BallActionType : byte
    {
        None = 0,
        Kick = 1,
        Hold = 2,
        Release = 3,
        Capture = 4,
    }

    [MessagePackObject]
    public struct BallSharedState : INetworkState
    {
        public static BallSharedState Default => new(0, 0, 0, 0, BallActionType.None, Vector3.zero, 0);

        [Key(0)]
        public byte KicksCount { get; }

        [Key(1)]
        public uint OwnerNetId { get; }

        [Key(2)]
        public uint HolderNetId { get; }

        [Key(3)]
        public uint LastActionId { get; }

        [Key(4)]
        public BallActionType LastActionType { get; }

        [Key(5)]
        public Vector3 LastKickDirection { get; }

        [Key(6)]
        public uint LastKickInitiatorNetId { get; }

        [IgnoreMember]
        public bool HasHolder => HolderNetId != 0;

        public BallSharedState(
            byte kicksCount,
            uint ownerNetId,
            uint holderNetId,
            uint lastActionId,
            BallActionType lastActionType,
            Vector3 lastKickDirection,
            uint lastKickInitiatorNetId
        )
        {
            KicksCount = kicksCount;
            OwnerNetId = ownerNetId;
            HolderNetId = holderNetId;
            LastActionId = lastActionId;
            LastActionType = lastActionType;
            LastKickDirection = lastKickDirection;
            LastKickInitiatorNetId = lastKickInitiatorNetId;
        }
    }
}
