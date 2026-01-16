using System;
using System.Collections.Generic;
using Gameplay.Interactable.BoxesInteraction;
using Gameplay.Interactable.BoxesInteraction.Abstract;
using Infrastructure.InputService.Abstract;
using Reflex.Attributes;
using UnityEngine;
using UnityEngine.AI;

namespace Configuration.Mediator
{
    [Serializable]
    public class BoxesInteractionMediator : IBoxesInteractionMediator
    {
        [SerializeField]
        private BoxesInteractionConfig config;

        private IInputService inputService;

        private IBoxInteractionInitiator localInitiator;
        private Dictionary<uint, IBoxInteractionInitiator> initiators = new();

        BoxesInteractionConfig IBoxesInteractionMediator.Config => config;
        IBoxInteractionInitiator IBoxesInteractionMediator.LocalInitiator => localInitiator;
        IReadOnlyDictionary<uint, IBoxInteractionInitiator> IBoxesInteractionMediator.Initiators => initiators;

        [Inject]
        private void Construct(IInputService inputService)
        {
            this.inputService = inputService;
        }

        void IBoxesInteractionMediator.RegisterInitiator(IBoxInteractionInitiator initiator, bool isLocal)
        {
            if (isLocal)
            {
                localInitiator = initiator;
            }

            initiators.Add(initiator.NetId, initiator);
        }

        Vector3 IBoxesInteractionMediator.GetThrowVelocity(IBoxInteractionInitiator initiator)
        {
            float y = config.ThrowForce.y;
            float x = config.ThrowForce.x;
            Vector3 targetVelocity = (initiator.Socket.forward + Vector3.up * y) * x;
            targetVelocity += initiator.Controller.GetVelocity() * Mathf.Abs(config.HolderSpeedModifier);
            return targetVelocity;
        }

        bool IBoxesInteractionMediator.IsPredictionVisible(out Vector3 targetPosition)
        {
            targetPosition = Vector3.zero;

            if (localInitiator == null || localInitiator.CurrentBox == null)
            {
                return false;
            }

            Vector3 initialVelocity = ((IBoxesInteractionMediator)this).GetThrowVelocity(localInitiator);
            targetPosition = ((IBoxesInteractionMediator)this).CalculateLandingPoint(
                localInitiator.Socket.position, initialVelocity
            );

            return localInitiator.CurrentBox != null && inputService.DefaultContextActions.Aim.IsPressed();
        }

        Vector3 IBoxesInteractionMediator.CalculateLandingPoint(Vector3 startPosition, Vector3 initialVelocity)
        {
            float gravity = Physics.gravity.y + config.ExtraGravity / 10;

            float timeToLand = 0;
            if (Mathf.Abs(gravity) > Mathf.Epsilon)
            {
                float gravityFactor = 0.5f * gravity;
                float initialVerticalVelocity = initialVelocity.y;
                float initialVerticalPosition = startPosition.y;

                float discriminant =
                    initialVerticalVelocity * initialVerticalVelocity - 4 * gravityFactor * initialVerticalPosition;

                if (discriminant >= 0)
                {
                    float sqrtDiscriminant = Mathf.Sqrt(discriminant);
                    float t1 = (-initialVerticalVelocity + sqrtDiscriminant) / (2 * gravityFactor);
                    float t2 = (-initialVerticalVelocity - sqrtDiscriminant) / (2 * gravityFactor);

                    timeToLand = Mathf.Max(t1, t2);
                }
            }

            Vector3 horizontalVelocity = new Vector3(initialVelocity.x, 0, initialVelocity.z);
            Vector3 expectedPosition = startPosition + horizontalVelocity * timeToLand;
            if (NavMesh.SamplePosition(expectedPosition, out NavMeshHit hit, 10, NavMesh.AllAreas))
            {
                return hit.position;
            }
            return expectedPosition;
        }
    }
}
