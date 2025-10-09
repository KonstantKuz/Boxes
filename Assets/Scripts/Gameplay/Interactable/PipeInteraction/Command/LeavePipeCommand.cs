using Infrastructure.Network.Abstract;
using MessagePack;

namespace Gameplay.Interactable.PipeInteraction.Command
{
    [MessagePackObject]
    public class LeavePipeCommand : INetworkCommand
    {
        [Key(0)]
        public readonly uint PlayerNetId;

        [Key(1)]
        public readonly uint PipeNetId;

        public LeavePipeCommand(uint playerNetId, uint pipeNetId)
        {
            PlayerNetId = playerNetId;
            PipeNetId = pipeNetId;
        }
    }
}
