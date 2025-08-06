using Infrastructure.InteractionService.Abstract;
using MessagePack;

namespace Gameplay.Interactable.Dialog
{
    [MessagePackObject]
    public class DialogInteractionContext : InteractionContext
    {
        [Key(0)]
        public readonly uint InitiatorNetId;

        public DialogInteractionContext(uint initiatorNetId)
        {
            InitiatorNetId = initiatorNetId;
        }
    }
}
