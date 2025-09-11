using Gameplay.Interactable.BoxesInteraction.Abstract;
using Gameplay.Interactable.BoxesInteraction.Command;
using Gameplay.Interactable.BoxesInteraction.State;
using Infrastructure.Network.Abstract;
using Mirror;
using Reflex.Attributes;
using UnityEngine;

namespace Gameplay.Interactable.BoxesInteraction.Components
{
    public class Box : NetworkBehaviour
    {
        [SerializeField]
        private BoxStateHolder stateHolder;

        [SerializeField]
        private new Rigidbody rigidbody;

        [SerializeField]
        private new Collider collider;

        private IBoxesInteractionMediator boxesInteractionMediator;
        private INetworkService networkService;

        public INetworkStateHolder<BoxSharedState> StateHolder => stateHolder;
        public BoxSharedState State => stateHolder.GetStateOrDefault<BoxSharedState>();
        public Rigidbody Rigidbody => rigidbody;
        public Collider Collider => collider;

        [Inject]
        private void Construct(IBoxesInteractionMediator boxesInteractionMediator, INetworkService networkService)
        {
            this.boxesInteractionMediator = boxesInteractionMediator;
            this.networkService = networkService;
        }

        private void Awake()
        {
            GetComponent<ConstantForce>().force = Vector3.up * boxesInteractionMediator.Config.ExtraGravity;
        }

        public override void OnStartServer()
        {
            networkService.ObserveToExecute<TakeBoxCommand>(ExecuteTake);
            networkService.ObserveToExecute<ReleaseBoxCommand>(ExecuteRelease);
            networkService.ObserveToExecute<ThrowBoxCommand>(ExecuteThrow);
        }

        private void ExecuteTake(TakeBoxCommand context)
        {
            if (context.TargetNetId == netId)
            {
                StateHolder.WriteState(new BoxSharedState(context.InitiatorNetId));
            }
        }

        private void ExecuteRelease(ReleaseBoxCommand context)
        {
            if (context.TargetNetId == netId)
            {
                StateHolder.WriteState(BoxSharedState.Default);
            }
        }

        private void ExecuteThrow(ThrowBoxCommand context)
        {
            bool hasValidHolder = boxesInteractionMediator.Initiators.TryGetValue(
                State.HolderNetId, out IBoxInteractionInitiator holdInitiator
            );

            if (context.TargetNetId == netId && hasValidHolder)
            {
                rigidbody.isKinematic = false;
                float y = boxesInteractionMediator.Config.ThrowForce.y;
                float x = boxesInteractionMediator.Config.ThrowForce.x;
                rigidbody.velocity = (holdInitiator.Socket.forward + Vector3.up * y) * x;
                StateHolder.WriteState(BoxSharedState.Default);
            }
        }

        public bool TryTake(uint initiatorNetId)
        {
            bool hasValidHolder = boxesInteractionMediator.Initiators.TryGetValue(
                State.HolderNetId, out IBoxInteractionInitiator holdInitiator
            );

            if (!hasValidHolder)
            {
                networkService.SendCommand(new TakeBoxCommand(initiatorNetId, netId));
                return true;
            }

            return false;
        }

        public bool TryRelease(uint initiatorNetId)
        {
            bool hasValidHolder = boxesInteractionMediator.Initiators.TryGetValue(
                State.HolderNetId, out IBoxInteractionInitiator holdInitiator
            );

            if (hasValidHolder && State.HolderNetId == initiatorNetId)
            {
                networkService.SendCommand(new ReleaseBoxCommand(initiatorNetId, netId));
                return true;
            }

            return false;
        }

        public bool TryThrow(uint initiatorNetId)
        {
            bool hasValidHolder = boxesInteractionMediator.Initiators.TryGetValue(
                State.HolderNetId, out IBoxInteractionInitiator holdInitiator
            );

            if (hasValidHolder && State.HolderNetId == initiatorNetId)
            {
                networkService.SendCommand(new ThrowBoxCommand(initiatorNetId, netId));
                return true;
            }

            return false;
        }

        private void Update()
        {
            rigidbody.isKinematic = collider.isTrigger = State.HasHolder;
        }

        private void FixedUpdate()
        {
            if (!isServer)
            {
                return;
            }

            bool hasValidHolder = boxesInteractionMediator.Initiators.TryGetValue(
                State.HolderNetId, out IBoxInteractionInitiator holdInitiator
            );

            if (hasValidHolder)
            {
                rigidbody.MovePosition(holdInitiator.Socket.position);
            }
        }
    }
}
