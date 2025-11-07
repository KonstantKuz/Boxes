using System.Threading;
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
        private CancellationTokenSource cancellation;

        uint IBallInteractionInitiator.NetId => netId;
        Transform IBallInteractionInitiator.BallSocket => ballSocket;
        Vector3 IBallInteractionInitiator.Position => transform.position;
        Vector3 IBallInteractionInitiator.KickDirection => kickDirectionRoot.forward;

        [Inject]
        private void Construct(IBallInteractionMediator ballInteractionMediator)
        {
            this.ballInteractionMediator = ballInteractionMediator;
        }

        public override void OnStartClient()
        {
            ballInteractionMediator.RegisterInitiator(this, isLocalPlayer);
        }

        private void OnDestroy()
        {
            cancellation.Cancel();
            cancellation.Dispose();
        }
    }
}
