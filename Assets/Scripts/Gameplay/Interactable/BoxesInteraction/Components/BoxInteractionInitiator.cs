using System;
using System.Linq;
using CMF;
using Gameplay.Interactable.Abstract;
using Gameplay.Interactable.BoxesInteraction.Abstract;
using Gameplay.Interactable.BoxesInteraction.State;
using Infrastructure.InputService.Abstract;
using R3;
using Reflex.Attributes;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

namespace Gameplay.Interactable.BoxesInteraction.Components
{
    public class BoxInteractionInitiator : InputAwareInteractionInitiator, IBoxInteractionInitiator
    {
        [SerializeField]
        private UnityEvent<float> OnSpeedModifierChanged;

        [SerializeField]
        private UnityEvent onBoxThrown;

        [SerializeField]
        private Transform socket;

        [SerializeField]
        private Rigidbody rigidbody;

        [SerializeField]
        private AdvancedWalkerController walkerController;

        private IBoxesInteractionMediator boxesInteractionMediator;
        private Box currentBox;
        private IDisposable stateSubscription;

        uint IBoxInteractionInitiator.NetId => netId;
        Transform IBoxInteractionInitiator.Socket => socket;
        Box IBoxInteractionInitiator.CurrentBox => currentBox;
        public Vector3 SafePosition => transform.position;
        public AdvancedWalkerController Controller => walkerController;
        bool IBoxInteractionInitiator.IsAimPressed => selfInput?.DefaultContext.Aim.IsPressed() ?? false;

        [Inject]
        private void ConstructBox(IBoxesInteractionMediator boxesInteractionMediator)
        {
            this.boxesInteractionMediator = boxesInteractionMediator;
        }

        protected override void OnStartClientInitiator()
        {
            boxesInteractionMediator.RegisterInitiator(this, isOwned);
        }

        protected override void SubscribeSelfInput(GameInputActions actions)
        {
            actions.DefaultContext.Take.performed += TryHoldOrRelease;
            actions.DefaultContext.Action.performed += TryThrow;
        }

        protected override void OnUnsubscribeSelfInput(GameInputActions actions)
        {
            actions.DefaultContext.Take.performed -= TryHoldOrRelease;
            actions.DefaultContext.Action.performed -= TryThrow;
        }

        private void TryHoldOrRelease(InputAction.CallbackContext context)
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
                .OrderBy(hit => Vector3.Distance(hit.transform.position, transform.position))
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
                stateSubscription = box.StateHolder.Subscribe(OnBoxStateChanged);
            }
        }

        private void TryThrow(InputAction.CallbackContext context)
        {
            if (currentBox != null)
            {
                BoxSharedState current = currentBox.StateHolder.GetState();
                Vector3 throwVelocity = boxesInteractionMediator.GetThrowVelocity(this);

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

        private void OnBoxStateChanged(BoxSharedState state)
        {
            if (state.LastActionType == BoxActionType.Throw && state.OwnerNetId == netId)
            {
                onBoxThrown.Invoke();
            }

            if (state.OwnerNetId == netId && state.LastActionType != BoxActionType.Hold)
            {
                stateSubscription?.Dispose();
            }
        }

        private void SetSpeedModifierActive(bool active)
        {
            float value = boxesInteractionMediator.Config.HolderSpeedModifier;
            OnSpeedModifierChanged.Invoke(active ? value : -value);
        }
    }
}
