using Infrastructure.DialogService;
using Infrastructure.DialogService.Abstract;
using Mirror;
using Reflex.Attributes;
using UnityEngine;

namespace Gameplay.Interactable.Dialog
{
    public class DialogOwner : NetworkBehaviour
    {
        [SerializeField]
        private DialogSequence dialogSequence;

        private IDialogService dialogService;

        [Inject]
        private void Construct(IDialogService dialogService)
        {
            this.dialogService = dialogService;
        }

        public void StartDialog(uint initiatorId)
        {
            dialogService.StartDialog(initiatorId, dialogSequence.Id);
        }
    }
}
