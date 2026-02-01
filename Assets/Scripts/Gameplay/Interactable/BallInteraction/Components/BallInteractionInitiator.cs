using System;
using Gameplay.Interactable.Abstract;
using Gameplay.Interactable.BallInteraction.Abstract;
using Gameplay.Interactable.BallInteraction.State;
using Infrastructure.InputService.Abstract;
using R3;
using Reflex.Attributes;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

namespace Gameplay.Interactable.BallInteraction.Components
{
    public class BallInteractionInitiator : InputAwareInteractionInitiator, IBallInteractionInitiator
    {
        [SerializeField]
        private UnityEvent<float> onKickSideChanged;

        [SerializeField]
        private UnityEvent onBallKicked;

        [SerializeField]
        private Transform kickDirectionRoot;

        [SerializeField]
        private Transform ballSocket;

        [SerializeField]
        private new Rigidbody rigidbody;

        private IBallInteractionMediator ballInteractionMediator;
        private IDisposable stateSubscription;
        private uint lastProcessedActionId;

        uint IBallInteractionInitiator.NetId => netId;
        Transform IBallInteractionInitiator.BallSocket => ballSocket;
        Vector3 IBallInteractionInitiator.Position => transform.position;
        Vector3 IBallInteractionInitiator.KickDirection => kickDirectionRoot.forward;
        Rigidbody IBallInteractionInitiator.Rigidbody => rigidbody;
        bool IBallInteractionInitiator.IsAimPressed => selfInput?.DefaultContext.Aim.IsPressed() ?? false;

        [Inject]
        private void ConstructBall(IBallInteractionMediator ballInteractionMediator)
        {
            this.ballInteractionMediator = ballInteractionMediator;
        }

        protected override void OnStartClientInitiator()
        {
            ballInteractionMediator.RegisterInitiator(this, isOwned);
            stateSubscription = ballInteractionMediator.BallState.Subscribe(OnStateChanged);
        }

        protected override void OnStopClientInitiator()
        {
            stateSubscription?.Dispose();
        }

        protected override void SubscribeSelfInput(GameInputActions actions)
        {
            actions.DefaultContext.Take.performed += OnTakePressed;
            actions.DefaultContext.Action.performed += OnActionPressed;
        }

        protected override void OnUnsubscribeSelfInput(GameInputActions actions)
        {
            actions.DefaultContext.Take.performed -= OnTakePressed;
            actions.DefaultContext.Action.performed -= OnActionPressed;
        }

        private void OnTakePressed(InputAction.CallbackContext context)
        {
            ballInteractionMediator.TryHoldOrReleaseBall(this);
        }

        private void OnActionPressed(InputAction.CallbackContext context)
        {
            ballInteractionMediator.TryKickBall(this);
        }

        private void OnStateChanged(BallSharedState state)
        {
            if (state.LastActionId == lastProcessedActionId)
            {
                return;
            }

            if (state.LastActionType != BallActionType.Kick || state.LastKickInitiatorNetId != netId)
            {
                lastProcessedActionId = state.LastActionId;
                return;
            }

            Vector3 localPosition = ballSocket.InverseTransformPoint(ballInteractionMediator.Ball.transform.position);
            float kickSide = localPosition.x >= 0 ? 1f : -1f;

            onKickSideChanged?.Invoke(kickSide);
            onBallKicked?.Invoke();

            lastProcessedActionId = state.LastActionId;
        }
    }
}
