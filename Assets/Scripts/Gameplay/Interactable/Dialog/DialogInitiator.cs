using System.Linq;
using Gameplay.Interactable.Abstract;
using Infrastructure.InputService.Abstract;
using Reflex.Attributes;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Gameplay.Interactable.Dialog
{
    public class DialogInitiator : InputAwareInteractionInitiator
    {
        [SerializeField]
        private float interactionDistance;

        protected override void SubscribeSelfInput(GameInputActions actions)
        {
            actions.DefaultContext.Interact.performed += TryInteract;
        }

        protected override void OnUnsubscribeSelfInput(GameInputActions actions)
        {
            actions.DefaultContext.Interact.performed -= TryInteract;
        }

        private void TryInteract(InputAction.CallbackContext context)
        {
            DialogOwner dialogOwner = GetInteractablesAround(interactionDistance)
                .Select(hit => hit.GetComponent<DialogOwner>())
                .FirstOrDefault(owner => owner);

            if (dialogOwner == null)
            {
                return;
            }

            dialogOwner.StartDialog(netIdentity.netId);
        }
    }
}
