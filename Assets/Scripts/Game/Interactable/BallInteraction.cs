using System;
using System.Collections.Generic;
using System.Linq;
using Infrastructure.InputService;
using Infrastructure.InteractionService;
using Mirror;
using R3;
using Reflex.Attributes;
using UnityEngine;

namespace Game.Interactable
{
    public class BallInteraction : NetworkBehaviour, ICaptureInteractionContextRoot
    {
        [SerializeReference]
        private List<InteractionContext> ballInteractionContexts;

        [SerializeField]
        private float interactionDistance = 2f;

        [SerializeField]
        private Transform kickDirectionRoot;

        [SerializeField]
        private Transform ballSocket;

        private readonly Collider[] hits = new Collider[10];
        private IInputService inputService;
        private IDisposable inputSubscription;

        Transform ICaptureInteractionContextRoot.Socket => ballSocket;

        [Inject]
        private void Construct(IInputService inputService)
        {
            this.inputService = inputService;
        }

        public override void OnStartLocalPlayer()
        {
            inputSubscription = inputService.InteractionInput.Subscribe(_ => TryInteract());
        }

        public override void OnStopLocalPlayer()
        {
            inputSubscription?.Dispose();
            inputSubscription = null;
        }

        private void TryInteract()
        {
            for (int i = 0; i < hits.Length; i++)
            {
                hits[i] = null;
            }

            Physics.OverlapSphereNonAlloc(transform.position, interactionDistance, hits);

            IInteractable ball = hits
                .Where(hit => hit)
                .Select(hit => hit.GetComponent<Ball>())
                .FirstOrDefault(interactable => interactable);

            if (ball == null || ball.State is not BallState ballState)
            {
                return;
            }

            ball.CmdInteract(
                ballState.OwnerNetId != 0
                ? GetInteractionContext(typeof(KickInteractionContext))
                : GetInteractionContext(typeof(CaptureInteractionContext))
            );
        }

        private InteractionContext GetInteractionContext(Type type)
        {
            return type.Name switch
            {
                nameof(KickInteractionContext) =>
                    new KickInteractionContext(netIdentity.netId, kickDirectionRoot.forward + Vector3.up / 2, 25f),
                nameof(CaptureInteractionContext) => new CaptureInteractionContext(netId),
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        private void OnDrawGizmos()
        {
            Gizmos.DrawWireSphere(transform.position, interactionDistance);
        }
    }
}
