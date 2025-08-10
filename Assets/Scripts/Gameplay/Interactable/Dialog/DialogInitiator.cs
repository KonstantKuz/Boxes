using System.Linq;
using Gameplay.Interactable.Abstract;
using Infrastructure.InputService.Abstract;
using Reflex.Attributes;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Gameplay.Interactable.Dialog
{
    public class DialogInitiator : InteractionInitiatorBase
    {
        [SerializeField]
        private float interactionDistance;

        private IInputService inputService;

        [Inject]
        private void Construct(IInputService inputService)
        {
            this.inputService = inputService;
        }

        public override void OnStartLocalPlayer()
        {
            if (isLocalPlayer)
            {
                inputService.DefaultContextActions.Interact.performed += TryInteract;
            }
        }

        public override void OnStopLocalPlayer()
        {
            if (isLocalPlayer)
            {
                inputService.DefaultContextActions.Interact.performed -= TryInteract;
            }
        }

        private void TryInteract(InputAction.CallbackContext ctx)
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
