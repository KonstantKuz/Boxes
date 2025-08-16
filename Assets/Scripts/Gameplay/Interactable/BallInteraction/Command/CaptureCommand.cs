using Infrastructure.Network.Abstract;
using MessagePack;
using UnityEngine;

namespace Gameplay.Interactable.BallInteraction.Command
{
    [MessagePackObject]
    public class CaptureCommand : INetworkCommand
    {
        [Key(0)]
        public readonly uint InitiatorNetId;

        [Key(1)]
        public readonly Vector3 RelativePosition;

        public CaptureCommand(uint initiatorNetId, Vector3 relativePosition)
        {
            RelativePosition = relativePosition;
            InitiatorNetId = initiatorNetId;
        }
    }
}
