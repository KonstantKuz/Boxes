using Infrastructure.DialogService;
using Infrastructure.InteractionService;
using Mirror;
using Reflex.Attributes;
using UnityEngine;

namespace Game.Interactable.Dialog
{
    public class DialogStarter : NetworkBehaviour, IInteractable
    {
        [SerializeField]
        private DialogSequence dialogSequence;

        private IDialogService dialogService;

        public InteractableState State => null;

        [Inject]
        private void Construct(IDialogService dialogService)
        {
            this.dialogService = dialogService;
        }

        [Command(requiresAuthority = false)]
        void IInteractable.CmdInteract(InteractionContext interactionContext)
        {
            dialogService.CmdStartDialog(dialogSequence.Id);
        }
    }
}
