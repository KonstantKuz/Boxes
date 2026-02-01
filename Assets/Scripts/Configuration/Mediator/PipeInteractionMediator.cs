using System;
using System.Collections.Generic;
using Gameplay.Interactable.Abstract;
using Gameplay.Interactable.PipeInteraction;
using Gameplay.Interactable.PipeInteraction.Abstract;
using Gameplay.Interactable.PipeInteraction.Command;
using Gameplay.Interactable.PipeInteraction.State;
using Infrastructure.Bootstrap;
using Infrastructure.Network.Abstract;
using R3;
using Reflex.Attributes;
using UnityEngine;

namespace Configuration.Mediator
{
    [Serializable]
    public class PipeInteractionMediator : InteractionMediatorBase<IPipeInteractionInitiator>, IPipeInteractionMediator, IInitializable
    {
        [SerializeField]
        private float interactionDistance = 2f;

        private INetworkService networkService;
        private ReactiveProperty<PipeSharedState> stateReactive;

        private Pipe pipe;

        Pipe IPipeInteractionMediator.Pipe => pipe;
        ReadOnlyReactiveProperty<PipeSharedState> IPipeInteractionMediator.PipeState => stateReactive;
        IPipeInteractionInitiator IPipeInteractionMediator.LocalInitiator => localInitiator;
        IReadOnlyDictionary<uint, IPipeInteractionInitiator> IPipeInteractionMediator.Initiators => initiators;

        [Inject]
        private void Construct(INetworkService networkService)
        {
            this.networkService = networkService;

            stateReactive = new ReactiveProperty<PipeSharedState>(PipeSharedState.Default);
        }

        void IInitializable.Initialize()
        {
        }

        void IPipeInteractionMediator.RegisterPipe(Pipe pipe)
        {
            this.pipe = pipe;
            this.pipe.StateHolder.Subscribe(value => stateReactive.Value = value);
        }

        void IPipeInteractionMediator.RegisterInitiator(IPipeInteractionInitiator initiator, bool isLocalPlayer)
        {
            RegisterInitiatorInternal(initiator, initiator.NetId, isLocalPlayer);
        }

        void IPipeInteractionMediator.UpdatePipeInput(IPipeInteractionInitiator initiator, Vector2 moveInput)
        {
            if (pipe == null || initiator == null)
            {
                return;
            }

            PipeSharedState state = pipe.State;

            if (!state.HasPlayer(initiator.NetId))
            {
                return;
            }

            Vector2 networkInput = state.PlayerInputs[initiator.NetId];

            if (!Mathf.Approximately(moveInput.x, networkInput.x) || !Mathf.Approximately(moveInput.y, networkInput.y))
            {
                networkService.SendCommand(new UpdatePipeInputCommand(initiator.NetId, pipe.netId, moveInput));
            }
        }

        void IPipeInteractionMediator.TryInteractWithPipe(IPipeInteractionInitiator initiator)
        {
            if (pipe == null || initiator == null)
            {
                return;
            }

            PipeSharedState state = pipe.State;

            if (state.HasPlayer(initiator.NetId))
            {
                pipe.TryLeave(initiator.NetId);
            }
            else
            {
                float distance = Vector3.Distance(initiator.Rigidbody.position, pipe.transform.position);
                if (distance <= interactionDistance)
                {
                    pipe.TryJoin(initiator.NetId);
                }
            }
        }

        bool IPipeInteractionMediator.IsLocalInitiator(uint netId)
        {
            return IsLocalInitiator(netId);
        }
    }
}
