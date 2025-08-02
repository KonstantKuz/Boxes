using System;
using Infrastructure;
using Infrastructure.InteractionService.Abstract;
using Mirror;

namespace Gameplay.Interactable.Dialog
{
    [Serializable]
    public class DialogInteractionContext : InteractionContext
    {
        public readonly uint InitiatorNetId;

        public DialogInteractionContext()
        {

        }

        public DialogInteractionContext(uint initiatorNetId)
        {
            InitiatorNetId = initiatorNetId;
        }

        public static InteractionContext Read(NetworkReader reader)
        {
            return new DialogInteractionContext(reader.ReadUInt());
        }

        public override void Write(NetworkWriter writer)
        {
            writer.WriteByte(TypeByteMapper.GetByteFromType(GetType()));
            writer.WriteUInt(InitiatorNetId);
        }
    }
}
