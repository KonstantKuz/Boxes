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
        public readonly Vector3 Direction;

        public KickCommand(uint initiatorNetId, Vector3 direction)
        {
            InitiatorNetId = initiatorNetId;
            Direction = direction;
        }
    }
}
