using System.Collections.Generic;
using System.Linq;
using Infrastructure.InputService.Abstract;
using Mirror;
using Reflex.Attributes;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Gameplay.Interactable.Abstract
{
    public abstract class InteractionInitiatorBase : NetworkBehaviour
    {
        [SerializeField]
        protected float interactionDistance = 1.3f;

        private readonly Collider[] hits = new Collider[10];

        private IInputService inputService;

        [Inject]
        private void Construct(IInputService inputService)
        {
            this.inputService = inputService;
        }

        public override void OnStartLocalPlayer()
        {
            inputService.InteractAction.performed += TryInteract;
        }

        public override void OnStopLocalPlayer()
        {
            inputService.InteractAction.performed -= TryInteract;
        }

        protected abstract void TryInteract(InputAction.CallbackContext ctx);

        protected IEnumerable<Collider> GetInteractablesAround()
        {
            for (int i = 0; i < hits.Length; i++)
            {
                hits[i] = null;
            }

            Physics.OverlapSphereNonAlloc(transform.position, interactionDistance, hits);

            return hits.Where(hit => hit);
        }

        private void OnDrawGizmos()
        {
            Gizmos.DrawWireSphere(transform.position, interactionDistance);
        }
    }
}
