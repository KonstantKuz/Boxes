using Infrastructure.Network.Abstract;
using MessagePack;
using UnityEngine;

namespace Gameplay.Interactable.PipeInteraction.Command
{
    [MessagePackObject]
    public class UpdatePipeInputCommand : INetworkCommand
    {
        [Key(0)]
        public readonly uint PlayerNetId;

        [Key(1)]
        public readonly uint PipeNetId;

        [Key(2)]
        public readonly Vector2 Input;

        public UpdatePipeInputCommand(uint playerNetId, uint pipeNetId, Vector2 input)
        {
            PlayerNetId = playerNetId;
            PipeNetId = pipeNetId;
            Input = input;
        }
    }
}
