using Infrastructure.Network.Abstract;
using MessagePack;
using UnityEngine;

namespace Gameplay.Interactable.BallInteraction.Command
{
    [MessagePackObject]
    public class KickCommand : INetworkCommand
    {
        [Key(0)]
        public readonly uint InitiatorNetId;

        [Key(1)]
        public readonly Vector3 KickDirection;

        [Key(2)]
        public readonly float KickForce;

        public KickCommand(uint initiatorNetId, Vector3 kickDirection, float kickForce = 0)
        {
            InitiatorNetId = initiatorNetId;
            KickDirection = kickDirection;
            KickForce = kickForce;
        }
    }
}
