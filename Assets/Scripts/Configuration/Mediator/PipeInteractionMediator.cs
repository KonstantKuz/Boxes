using System;
using System.Collections.Generic;
using Gameplay.Interactable.PipeInteraction;
using Gameplay.Interactable.PipeInteraction.Abstract;
using Gameplay.Interactable.PipeInteraction.Command;
using Gameplay.Interactable.PipeInteraction.State;
using Infrastructure.Bootstrap;
using Infrastructure.InputService.Abstract;
using Infrastructure.Network.Abstract;
using R3;
using Reflex.Attributes;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Configuration.Mediator
{
    [Serializable]
    public class PipeInteractionMediator : IPipeInteractionMediator, IInitializable, IUpdatable
    {
        [SerializeField]
        private float interactionDistance = 2f;

        private IInputService inputService;
        private INetworkService networkService;
        private ReactiveProperty<PipeSharedState> stateReactive;
        private Dictionary<uint, IPipeInteractionInitiator> initiators;

        private Pipe pipe;
        private IPipeInteractionInitiator localInitiator;

        ReadOnlyReactiveProperty<PipeSharedState> IPipeInteractionMediator.PipeState => stateReactive;
        IReadOnlyDictionary<uint, IPipeInteractionInitiator> IPipeInteractionMediator.Initiators => initiators;

        [Inject]
        private void Construct(IInputService inputService, INetworkService networkService)
        {
            this.inputService = inputService;
            this.networkService = networkService;

            stateReactive = new ReactiveProperty<PipeSharedState>(PipeSharedState.Default);
            initiators = new Dictionary<uint, IPipeInteractionInitiator>();
        }

        void IInitializable.Initialize()
        {
            inputService.DefaultContextActions.Interact.performed += TryInteractWithPipe;
        }

        void IPipeInteractionMediator.RegisterPipe(Pipe pipe)
        {
            this.pipe = pipe;
            this.pipe.StateHolder.Subscribe(value => stateReactive.Value = value);
        }

        void IPipeInteractionMediator.RegisterInitiator(IPipeInteractionInitiator initiator, bool isLocalPlayer)
        {
            if (isLocalPlayer)
            {
                localInitiator = initiator;
            }

            initiators.Add(initiator.NetId, initiator);
        }

        (Vector3 faceDirection, Vector3 position) IPipeInteractionMediator.GetTransformState(uint playerNetId)
        {
            return pipe == null
                ? (Vector3.zero, Vector3.zero)
                : (pipe.transform.forward, pipe.GetPlayerWorldPosition(playerNetId));
        }

        void IUpdatable.Update()
        {
            if (pipe == null || localInitiator == null)
            {
                return;
            }

            PipeSharedState state = pipe.State;

            if (!state.HasPlayer(localInitiator.NetId))
            {
                return;
            }

            Vector2 moveInput = inputService.DefaultContextActions.Move.ReadValue<Vector2>();
            Vector2 networkInput = state.PlayerInputs[localInitiator.NetId];

            if (!Mathf.Approximately(moveInput.x, networkInput.x) || !Mathf.Approximately(moveInput.y, networkInput.y))
            {
                networkService.SendCommand(new UpdatePipeInputCommand(localInitiator.NetId, pipe.netId, moveInput));
            }
        }

        private void TryInteractWithPipe(InputAction.CallbackContext context)
        {
            if (pipe == null || localInitiator == null)
            {
                return;
            }

            PipeSharedState state = pipe.State;

            if (state.HasPlayer(localInitiator.NetId))
            {
                pipe.TryLeave(localInitiator.NetId);
            }
            else
            {
                float distance = Vector3.Distance(localInitiator.Position, pipe.transform.position);
                if (distance <= interactionDistance)
                {
                    pipe.TryJoin(localInitiator.NetId);
                }
            }
        }
    }
}
