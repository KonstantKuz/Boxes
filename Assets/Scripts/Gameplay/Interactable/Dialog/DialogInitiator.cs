using System.Linq;
using Gameplay.Interactable.Abstract;
using Infrastructure.InteractionService.Abstract;
using UnityEngine.InputSystem;

namespace Gameplay.Interactable.Dialog
{
    public class DialogInitiator : InteractionInitiatorBase
    {
        protected override void TryInteract(InputAction.CallbackContext ctx)
        {
            IInteractable dialogOwner = GetInteractablesAround()
                .Select(hit => hit.GetComponent<DialogOwner>())
                .FirstOrDefault(owner => owner);

            if (dialogOwner == null)
            {
                return;
            }

            dialogOwner.CmdInteract(new DialogInteractionContext(netIdentity.netId));
        }
    }
}
