using System;
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
        private Rigidbody rigidbody;

        public uint NetId => netId;
        public Rigidbody Rigidbody => rigidbody ??= GetComponent<Rigidbody>();

        [Inject]
        private void Construct(IPipeInteractionMediator mediator)
        {
            this.mediator = mediator;
        }

        public override void OnStartClient()
        {
            mediator.RegisterInitiator(this, isLocalPlayer);

            stateSubscription = mediator.PipeState.Subscribe(OnPipeStateChanged);
        }

        public override void OnStopClient()
        {
            stateSubscription?.Dispose();
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
