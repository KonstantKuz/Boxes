using System;
using CMF;
using Gameplay.Interactable.PipeInteraction.Abstract;
using Gameplay.Interactable.PipeInteraction.State;
using Mirror;
using R3;
using Reflex.Attributes;
using UnityEngine;
using UnityEngine.Events;

namespace Gameplay.Interactable.PipeInteraction
{
    public class PipeInteractionInitiator : NetworkBehaviour, IPipeInteractionInitiator
    {
        [SerializeField]
        private UnityEvent<bool> isOnPipeChanged;

        private IPipeInteractionMediator mediator;
        private IDisposable stateSubscription;
        private bool isOnPipe;
        private TurnTowardControllerVelocity turnController;

        public uint NetId => netId;
        public Vector3 Position => transform.position;

        [Inject]
        private void Construct(IPipeInteractionMediator mediator)
        {
            this.mediator = mediator;
        }

        public override void OnStartClient()
        {
            turnController = GetComponentInChildren<TurnTowardControllerVelocity>();
            mediator.RegisterInitiator(this, isLocalPlayer);

            stateSubscription = mediator.PipeState.Subscribe(OnPipeStateChanged);
        }

        public override void OnStopClient()
        {
            stateSubscription?.Dispose();
        }

        private void Update()
        {
            if (!isOnPipe)
            {
                return;
            }

            (Vector3 faceDirection, Vector3 position) transformState = mediator.GetTransformState(netId);

            transform.position = Vector3.Lerp(transform.position, transformState.position, Time.deltaTime * 50);
            transform.forward = transformState.faceDirection;
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
    }
}
