using Infrastructure.Network.Abstract;
using MessagePack;

namespace Gameplay.Interactable.BoxesInteraction.Command
{
    [MessagePackObject]
    public class ReleaseBoxCommand : INetworkCommand
    {
        [Key(0)]
        public readonly uint InitiatorNetId;
        
        [Key(1)]
        public readonly uint TargetNetId;

        public ReleaseBoxCommand(uint initiatorNetId, uint targetNetId)
        {
            InitiatorNetId = initiatorNetId;
            TargetNetId = targetNetId;
        }
    }
}