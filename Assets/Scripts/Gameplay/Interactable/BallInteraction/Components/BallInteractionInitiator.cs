using Gameplay.Interactable.Abstract;
using Gameplay.Interactable.BallInteraction.Abstract;
using Reflex.Attributes;
using UnityEngine;

namespace Gameplay.Interactable.BallInteraction.Components
{
    public class BallInteractionInitiator : InteractionInitiatorBase, IBallInteractionInitiator
    {
        [SerializeField]
        private Transform kickDirectionRoot;

        [SerializeField]
        private Transform ballSocket;

        private IBallInteractionMediator ballInteractionMediator;

        uint IBallInteractionInitiator.NetId => netId;
        Transform IBallInteractionInitiator.BallSocket => ballSocket;
        Vector3 IBallInteractionInitiator.Position => transform.position;
        public Vector3 KickDirection => kickDirectionRoot.forward;

        [Inject]
        private void Construct(IBallInteractionMediator ballInteractionMediator)
        {
            this.ballInteractionMediator = ballInteractionMediator;
        }

        public override void OnStartClient()
        {
            ballInteractionMediator.RegisterInitiator(netId, this, isLocalPlayer);
        }
    }
}
