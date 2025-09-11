using Infrastructure.Network.Abstract;
using MessagePack;

namespace Gameplay.Interactable.BoxesInteraction.Command
{
    [MessagePackObject]
    public class TakeBoxCommand : INetworkCommand
    {
        [Key(0)]
        public readonly uint InitiatorNetId;

        [Key(1)]
        public readonly uint TargetNetId;

        public TakeBoxCommand(uint initiatorNetId, uint targetNetId)
        {
            InitiatorNetId = initiatorNetId;
            TargetNetId = targetNetId;
        }
    }
}
