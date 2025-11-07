using Gameplay.Interactable.BoxesInteraction.Abstract;
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

        private INetworkService networkService;
        private IBoxesInteractionMediator boxesInteractionMediator;

        private BoxSharedState? previousState;
        private BoxSharedState? pendingAction;

        public INetworkStateHolder<BoxSharedState> StateHolder => stateHolder;
        public BoxSharedState State => stateHolder.GetStateOrDefault<BoxSharedState>();
        public Rigidbody Rigidbody => rigidbody;
        public Collider Collider => collider;

        [Inject]
        private void Construct(INetworkService networkService, IBoxesInteractionMediator boxesInteractionMediator)
        {
            this.networkService = networkService;
            this.boxesInteractionMediator = boxesInteractionMediator;
        }

        private void Awake()
        {
            GetComponent<ConstantForce>().force = Vector3.up * boxesInteractionMediator.Config.ExtraGravity;
        }

        public override void OnStartServer()
        {
            StateHolder.Subscribe(OnStateChangedServer);
        }

        public override void OnStartClient()
        {
            StateHolder.Subscribe(OnStateChangedClient);

            if (isServer)
            {
                networkService.AssignAuthority(netIdentity);
            }
        }

        public override void OnStartAuthority()
        {
            if (pendingAction.HasValue)
            {
                HandleAction(pendingAction.Value);
                pendingAction = null;
            }
        }

        private void OnStateChangedServer(BoxSharedState newState)
        {
            if (!isServer)
            {
                return;
            }

            if (newState.OwnerNetId != previousState?.OwnerNetId && newState.OwnerNetId != 0)
            {
                networkService.AssignAuthority(netIdentity, newState.OwnerNetId);
            }
        }

        private void OnStateChangedClient(BoxSharedState newState)
        {
            if (newState.LastActionId != previousState?.LastActionId && newState.LastActionId > 0)
            {
                rigidbody.isKinematic = collider.isTrigger = State.HasHolder;

                IBoxInteractionInitiator localInitiator = boxesInteractionMediator.LocalInitiator;
                bool isLocal = localInitiator != null && newState.OwnerNetId == localInitiator.NetId;

                if (isOwned && isLocal)
                {
                    HandleAction(newState);
                    pendingAction = null;
                }
                else if (!isOwned && isLocal)
                {
                    pendingAction = newState;
                }
            }

            previousState = newState;
        }

        private void HandleAction(BoxSharedState state)
        {
            switch (state.LastActionType)
            {
                case BoxActionType.Throw:
                    ApplyThrowPhysics(state);
                    break;
            }
        }

        private void ApplyThrowPhysics(BoxSharedState state)
        {
            rigidbody.velocity = state.ThrowVelocity;
        }

        private void FixedUpdate()
        {
            if (!isOwned)
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
