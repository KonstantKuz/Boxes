using Infrastructure.DialogService;
using Infrastructure.DialogService.Abstract;
using Infrastructure.InteractionService.Abstract;
using Mirror;
using Reflex.Attributes;
using UnityEngine;

namespace Gameplay.Interactable.Dialog
{
    public class DialogOwner : NetworkBehaviour, IInteractable
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
