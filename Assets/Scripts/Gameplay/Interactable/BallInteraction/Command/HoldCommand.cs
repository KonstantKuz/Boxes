using Infrastructure.Network.Abstract;
using MessagePack;

namespace Gameplay.Interactable.BallInteraction.Command
{
    [MessagePackObject]
    public class HoldCommand : INetworkCommand
    {
        [Key(0)]
        public readonly uint InitiatorNetId;

        public HoldCommand(uint initiatorNetId)
        {
            InitiatorNetId = initiatorNetId;
        }
    }
}
