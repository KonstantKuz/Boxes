using System;
using Gameplay.Interactable.Abstract;
using Gameplay.Interactable.PipeInteraction.Abstract;
using Gameplay.Interactable.PipeInteraction.State;
using Infrastructure.InputService.Abstract;
using R3;
using Reflex.Attributes;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

namespace Gameplay.Interactable.PipeInteraction
{
    public class PipeInteractionInitiator : InputAwareInteractionInitiator, IPipeInteractionInitiator
    {
        [SerializeField]
        private UnityEvent<bool> isOnPipeChanged;

        private IPipeInteractionMediator mediator;
        private IDisposable stateSubscription;
        private bool isOnPipe;
        private Rigidbody rigidbody;

        public uint NetId => netId;
        public Rigidbody Rigidbody => rigidbody ??= GetComponent<Rigidbody>();

        [Inject]
        private void ConstructPipe(IPipeInteractionMediator mediator)
        {
            this.mediator = mediator;
        }

        protected override void OnStartClientInitiator()
        {
            mediator.RegisterInitiator(this, isOwned);
            stateSubscription = mediator.PipeState.Subscribe(OnPipeStateChanged);
        }

        protected override void OnStopClientInitiator()
        {
            stateSubscription?.Dispose();
        }

        protected override void SubscribeSelfInput(GameInputActions actions)
        {
            actions.DefaultContext.Interact.performed += OnInteractPressed;
        }

        protected override void OnUnsubscribeSelfInput(GameInputActions actions)
        {
            actions.DefaultContext.Interact.performed -= OnInteractPressed;
        }

        private void OnInteractPressed(InputAction.CallbackContext context)
        {
            mediator.TryInteractWithPipe(this);
        }

        private void Update()
        {
            if (selfInput != null && mediator.Pipe != null && mediator.PipeState.CurrentValue.HasPlayer(netId))
            {
                Vector2 moveInput = selfInput.DefaultContext.Move.ReadValue<Vector2>();
                mediator.UpdatePipeInput(this, moveInput);
            }
        }

        private void OnPipeStateChanged(PipeSharedState state)
        {
            bool wasOnPipe = isOnPipe;
            isOnPipe = state.HasPlayer(netId);

            if (wasOnPipe != isOnPipe)
            {
                isOnPipeChanged.Invoke(isOnPipe);
            }

            if (isOnPipe)
            {
                transform.GetChild(0).transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
            }
        }

        private void FixedUpdate()
        {
            if (mediator.PipeState.CurrentValue.HasPlayer(netId))
            {
                Rigidbody.MovePosition(mediator.Pipe.GetPlayerWorldPosition(netId));
                Rigidbody.MoveRotation(Quaternion.LookRotation(mediator.Pipe.transform.forward));
            }
        }
    }
}
