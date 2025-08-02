using System;
using System.Linq;
using Gameplay.Interactable.Abstract;
using Infrastructure.InteractionService.Abstract;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Gameplay.Interactable.BallInteraction
{
    public class BallInteractionInitiator : InteractionInitiatorBase, IBallInteractionInitiator
    {
        [SerializeField]
        private float kickForce = 15f;

        [SerializeField]
        private float verticalForceMultiplier = 0.3f;

        [SerializeField]
        private Transform kickDirectionRoot;

        [SerializeField]
        private Transform ballSocket;

        Transform IBallInteractionInitiator.BallSocket => ballSocket;

        protected override void TryInteract(InputAction.CallbackContext context)
        {
            IInteractable ball = GetInteractablesAround()
                .Select(hit => hit.GetComponent<Ball>())
                .FirstOrDefault(ball => ball);

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
                nameof(KickInteractionContext) => new KickInteractionContext(
                    netIdentity.netId,
                    kickDirectionRoot.forward + Vector3.up * verticalForceMultiplier,
                    kickForce
                ),
                nameof(CaptureInteractionContext) => new CaptureInteractionContext(netId),
                _ => throw new ArgumentOutOfRangeException()
            };
        }
    }
}
