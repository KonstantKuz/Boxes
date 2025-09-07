using System.Linq;
using Gameplay.Interactable.Abstract;
using Gameplay.Interactable.BoxesInteraction.Abstract;
using Infrastructure.InputService.Abstract;
using Reflex.Attributes;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

namespace Gameplay.Interactable.BoxesInteraction.Components
{
    public class BoxInteractionInitiator : InteractionInitiatorBase, IBoxInteractionInitiator
    {
        [SerializeField]
        private UnityEvent<float> OnSpeedModifierChanged;

        [SerializeField]
        private Transform socket;

        private IInputService inputService;
        private IBoxesInteractionMediator boxesInteractionMediator;
        private Box currentBox;

        uint IBoxInteractionInitiator.NetId => netId;
        Transform IBoxInteractionInitiator.Socket => socket;
        Box IBoxInteractionInitiator.CurrentBox => currentBox;

        [Inject]
        private void Construct(IInputService inputService, IBoxesInteractionMediator boxesInteractionMediator)
        {
            this.inputService = inputService;
            this.boxesInteractionMediator = boxesInteractionMediator;
        }

        public override void OnStartClient()
        {
            boxesInteractionMediator.RegisterInitiator(this, isLocalPlayer);
        }

        public override void OnStartLocalPlayer()
        {
            if (isLocalPlayer)
            {
                inputService.DefaultContextActions.Take.performed += TryTakeOrRelease;
                inputService.DefaultContextActions.Action.performed += TryThrow;
            }
        }

        public override void OnStopLocalPlayer()
        {
            if (isLocalPlayer)
            {
                inputService.DefaultContextActions.Take.performed -= TryTakeOrRelease;
                inputService.DefaultContextActions.Action.performed -= TryThrow;
            }
        }

        private void TryTakeOrRelease(InputAction.CallbackContext ctx)
        {
            if (currentBox != null && currentBox.TryRelease(netId))
            {
                currentBox = null;
                OnSpeedModifierChanged?.Invoke(1f);
                return;
            }

            Box box = GetInteractablesAround(boxesInteractionMediator.Config.InteractionDistance)
                .Select(hit => hit.GetComponent<Box>())
                .FirstOrDefault(owner => owner);

            if (box != null && box.TryTake(netIdentity.netId))
            {
                currentBox = box;
                OnSpeedModifierChanged?.Invoke(boxesInteractionMediator.Config.HolderSpeedModifier);
            }
        }

        private void TryThrow(InputAction.CallbackContext ctx)
        {
            if (currentBox != null && currentBox.TryThrow(netId))
            {
                currentBox = null;
                OnSpeedModifierChanged?.Invoke(1f);
            }
        }
    }
}
