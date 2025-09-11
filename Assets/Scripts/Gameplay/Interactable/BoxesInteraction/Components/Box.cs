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

        private IBoxesInteractionMediator boxesInteractionMediator;

        public INetworkStateHolder<BoxSharedState> StateHolder => stateHolder;
        public BoxSharedState State => stateHolder.GetStateOrDefault<BoxSharedState>();
        public Rigidbody Rigidbody => rigidbody;
        public Collider Collider => collider;

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
            bool hasValidHolder = boxesInteractionMediator.Initiators.TryGetValue(
                State.HolderNetId, out IBoxInteractionInitiator holdInitiator
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
            bool hasValidHolder = boxesInteractionMediator.Initiators.TryGetValue(
                State.HolderNetId, out IBoxInteractionInitiator holdInitiator
            );

            if (hasValidHolder && State.HolderNetId == initiatorNetId)
            {
                StateHolder.WriteState(BoxSharedState.Default);
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
