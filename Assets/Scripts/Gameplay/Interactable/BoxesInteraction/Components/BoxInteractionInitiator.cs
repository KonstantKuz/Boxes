using System.Linq;
using Gameplay.Interactable.Abstract;
using Gameplay.Interactable.BoxesInteraction.Abstract;
using Gameplay.Interactable.BoxesInteraction.State;
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
                inputService.DefaultContextActions.Take.performed += TryHoldOrRelease;
                inputService.DefaultContextActions.Action.performed += TryThrow;
            }
        }

        public override void OnStopLocalPlayer()
        {
            if (isLocalPlayer)
            {
                inputService.DefaultContextActions.Take.performed -= TryHoldOrRelease;
                inputService.DefaultContextActions.Action.performed -= TryThrow;
            }
        }

        private void TryHoldOrRelease(InputAction.CallbackContext ctx)
        {
            if (currentBox != null)
            {
                BoxSharedState current = currentBox.StateHolder.GetState();
                currentBox.StateHolder.WriteState(new BoxSharedState(
                    ownerNetId: netId,
                    holderNetId: 0,
                    lastActionId: current.LastActionId + 1,
                    lastActionType: BoxActionType.Release,
                    throwVelocity: Vector3.zero
                ));

                currentBox = null;
                SetSpeedModifierActive(false);
                return;
            }

            Box box = GetInteractablesAround(boxesInteractionMediator.Config.InteractionDistance)
                .Select(hit => hit.GetComponent<Box>())
                .Where(hit => hit != null)
                .OrderByDescending(hit => Vector3.Distance(hit.transform.position, transform.position))
                .FirstOrDefault(hit => hit);

            if (box != null && !box.State.HasHolder)
            {
                BoxSharedState current = box.StateHolder.GetState();
                box.StateHolder.WriteState(new BoxSharedState(
                    ownerNetId: netId,
                    holderNetId: netId,
                    lastActionId: current.LastActionId + 1,
                    lastActionType: BoxActionType.Hold,
                    throwVelocity: Vector3.zero
                ));

                currentBox = box;
                SetSpeedModifierActive(true);
            }
        }

        private void TryThrow(InputAction.CallbackContext ctx)
        {
            if (currentBox != null)
            {
                BoxSharedState current = currentBox.StateHolder.GetState();

                float y = boxesInteractionMediator.Config.ThrowForce.y;
                float x = boxesInteractionMediator.Config.ThrowForce.x;
                Vector3 throwVelocity = (socket.forward + Vector3.up * y) * x;

                currentBox.StateHolder.WriteState(new BoxSharedState(
                    ownerNetId: netId,
                    holderNetId: 0,
                    lastActionId: current.LastActionId + 1,
                    lastActionType: BoxActionType.Throw,
                    throwVelocity: throwVelocity
                ));

                currentBox = null;
                SetSpeedModifierActive(false);
            }
        }

        private void SetSpeedModifierActive(bool active)
        {
            float value = boxesInteractionMediator.Config.HolderSpeedModifier;
            OnSpeedModifierChanged.Invoke(active ? value : -value);
        }
    }
}
