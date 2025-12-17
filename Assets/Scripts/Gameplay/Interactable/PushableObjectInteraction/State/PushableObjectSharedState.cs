using Infrastructure.Network.Abstract;
using MessagePack;
using UnityEngine;

namespace Gameplay.Interactable.PushableObjectInteraction.State
{
    public enum PushableActionType : byte
    {
        None = 0,
        Kick = 1
    }

    [MessagePackObject]
    public struct PushableObjectSharedState : INetworkState
    {
        public static PushableObjectSharedState Default => new(0, PushableActionType.None, 0, Vector3.zero, Vector3.zero, 0f);

        [Key(0)]
        public uint LastActionId { get; }

        [Key(1)]
        public PushableActionType LastActionType { get; }

        [Key(2)]
        public uint LastKickInitiatorNetId { get; }

        [Key(3)]
        public Vector3 KickPosition { get; }

        [Key(4)]
        public Vector3 KickDirection { get; }

        [Key(5)]
        public float KickForce { get; }

        public PushableObjectSharedState(
            uint lastActionId,
            PushableActionType lastActionType,
            uint lastKickInitiatorNetId,
            Vector3 kickPosition,
            Vector3 kickDirection,
            float kickForce
        )
        {
            LastActionId = lastActionId;
            LastActionType = lastActionType;
            LastKickInitiatorNetId = lastKickInitiatorNetId;
            KickPosition = kickPosition;
            KickDirection = kickDirection;
            KickForce = kickForce;
        }
    }
}
