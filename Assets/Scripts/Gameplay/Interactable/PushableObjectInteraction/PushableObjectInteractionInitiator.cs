using System;
using System.Linq;
using Gameplay.Interactable.Abstract;
using Gameplay.Interactable.PushableObjectInteraction.Abstract;
using Gameplay.Interactable.PushableObjectInteraction.State;
using Infrastructure.InputService.Abstract;
using R3;
using Reflex.Attributes;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

namespace Gameplay.Interactable.PushableObjectInteraction
{
    public class PushableObjectInteractionInitiator : InputAwareInteractionInitiator, IPushableObjectInteractionInitiator
    {
        [SerializeField]
        private UnityEvent onObjectKicked;

        [SerializeField]
        private Transform socket;

        [SerializeField]
        private float interactionDistance = 3f;

        [SerializeField]
        private float kickForce = 10f;

        private IPushableObjectInteractionMediator mediator;
        private IDisposable stateSubscription;
        private uint lastProcessedActionId;

        public uint NetId => netId;

        [Inject]
        private void ConstructPushable(IPushableObjectInteractionMediator mediator)
        {
            this.mediator = mediator;
        }

        protected override void OnStartClientInitiator()
        {
            mediator.RegisterInitiator(this, isOwned);

            if (mediator.PushableObject != null)
            {
                stateSubscription = mediator.PushableObject.StateHolder.Subscribe(OnStateChanged);
            }
        }

        protected override void OnStopClientInitiator()
        {
            stateSubscription?.Dispose();
        }

        protected override void SubscribeSelfInput(GameInputActions actions)
        {
            actions.DefaultContext.Action.performed += TryKick;
        }

        protected override void OnUnsubscribeSelfInput(GameInputActions actions)
        {
            actions.DefaultContext.Action.performed -= TryKick;
        }

        private void OnStateChanged(PushableObjectSharedState state)
        {
            if (state.LastActionId == lastProcessedActionId)
            {
                return;
            }

            if (state.LastActionType != PushableActionType.Kick || state.LastKickInitiatorNetId != netId)
            {
                lastProcessedActionId = state.LastActionId;
                return;
            }

            onObjectKicked?.Invoke();

            lastProcessedActionId = state.LastActionId;
        }

        private void TryKick(InputAction.CallbackContext context)
        {
            PushableObject pushable = GetInteractablesAround(interactionDistance)
                .Select(hit => hit.GetComponentInParent<PushableObject>())
                .Where(hit => hit != null)
                .OrderBy(hit => Vector3.Distance(hit.transform.position, transform.position))
                .FirstOrDefault();

            if (pushable == null)
            {
                return;
            }

            PushableObjectSharedState current = pushable.StateHolder.GetState();

            Vector3 kickDirection = socket.forward.normalized;
            Vector3 toObject = Vector3.ProjectOnPlane(pushable.transform.position - socket.position, Vector3.up);

            if (Vector3.Dot(kickDirection, toObject.normalized) < -0.2f)
            {
                return;
            }

            Vector3 kickPosition = socket.position;

            pushable.StateHolder.WriteState(new PushableObjectSharedState(
                lastActionId: current.LastActionId + 1,
                lastActionType: PushableActionType.Kick,
                lastKickInitiatorNetId: netId,
                kickPosition: kickPosition,
                kickDirection: kickDirection,
                kickForce: kickForce
            ));
        }
    }
}
