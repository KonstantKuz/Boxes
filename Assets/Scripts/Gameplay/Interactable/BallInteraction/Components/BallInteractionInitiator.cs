using System.Threading;
using Cysharp.Threading.Tasks;
using Gameplay.Interactable.Abstract;
using Gameplay.Interactable.BallInteraction.Abstract;
using Mirror;
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

        // private void Awake()
        // {
        //     cancellation = new CancellationTokenSource();
        //
        //     UniTask.Void(async () =>
        //     {
        //         float awaitingTime = 0;
        //         while (!cancellation.IsCancellationRequested && NetworkClient.ready && netIdentity != null && netIdentity.netId == 0)
        //         {
        //             awaitingTime += Time.deltaTime;
        //             await UniTask.Yield();
        //
        //             if (awaitingTime > 5)
        //             {
        //                 Debug.LogWarning("No valid netId.");
        //                 awaitingTime = 0;
        //             }
        //         }
        //
        //         ballInteractionMediator.RegisterInitiator(this, isLocalPlayer);
        //     });
        // }

        private void OnDestroy()
        {
            cancellation.Cancel();
            cancellation.Dispose();
        }
    }
}
