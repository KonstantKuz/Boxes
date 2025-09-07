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
        private Rigidbody rigidbody;

        [SerializeField]
        private Collider collider;

        private IBoxesInteractionMediator boxesInteractionMediator;

        public INetworkStateHolder<BoxSharedState> StateHolder => stateHolder;
        public Rigidbody Rigidbody => rigidbody;

        [Inject]
        private void Construct(IBoxesInteractionMediator boxesInteractionMediator)
        {
            this.boxesInteractionMediator = boxesInteractionMediator;
        }

        private void Awake()
        {
            GetComponent<ConstantForce>().force = Vector3.up * boxesInteractionMediator.Config.ExtraGravity;
        }

        public bool TryTake(uint initiatorNetId)
        {
            BoxSharedState state = StateHolder.GetState();

            bool hasValidHolder = boxesInteractionMediator.Initiators.TryGetValue(
                state.HolderNetId, out IBoxInteractionInitiator holdInitiator
            );

            if (!hasValidHolder)
            {
                StateHolder.WriteState(new BoxSharedState(initiatorNetId));
                return true;
            }

            return false;
        }

        public bool TryRelease(uint initiatorNetId)
        {
            BoxSharedState state = StateHolder.GetState();

            bool hasValidHolder = boxesInteractionMediator.Initiators.TryGetValue(
                state.HolderNetId, out IBoxInteractionInitiator holdInitiator
            );

            if (hasValidHolder && state.HolderNetId == initiatorNetId)
            {
                StateHolder.WriteState(BoxSharedState.Default);
                return true;
            }

            return false;
        }

        public bool TryThrow(uint initiatorNetId)
        {
            BoxSharedState state = StateHolder.GetState();

            bool hasValidHolder = boxesInteractionMediator.Initiators.TryGetValue(
                state.HolderNetId, out IBoxInteractionInitiator holdInitiator
            );

            if (hasValidHolder && state.HolderNetId == initiatorNetId)
            {
                rigidbody.isKinematic = false;
                float y = boxesInteractionMediator.Config.ThrowForce.y;
                float x = boxesInteractionMediator.Config.ThrowForce.x;
                rigidbody.velocity = (holdInitiator.Socket.forward + Vector3.up * y) * x;
                StateHolder.WriteState(BoxSharedState.Default);
                return true;
            }

            return false;
        }

        private void Update()
        {
            bool hasHolder = stateHolder.GetStateOrDefault<BoxSharedState>().HasHolder;
            rigidbody.isKinematic = collider.isTrigger = hasHolder;
        }

        private void FixedUpdate()
        {
            if (!isServer)
            {
                return;
            }

            BoxSharedState state = stateHolder.GetStateOrDefault<BoxSharedState>();

            bool hasValidHolder = boxesInteractionMediator.Initiators.TryGetValue(
                state.HolderNetId, out IBoxInteractionInitiator holdInitiator
            );

            if (hasValidHolder)
            {
                rigidbody.MovePosition(holdInitiator.Socket.position);
            }
        }
    }
}
