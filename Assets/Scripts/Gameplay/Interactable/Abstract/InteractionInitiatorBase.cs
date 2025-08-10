using System.Collections.Generic;
using System.Linq;
using Mirror;
using UnityEngine;

namespace Gameplay.Interactable.Abstract
{
    public abstract class InteractionInitiatorBase : NetworkBehaviour
    {
        private readonly Collider[] hits = new Collider[10];

        public IEnumerable<Collider> GetInteractablesAround(float distance)
        {
            for (int i = 0; i < hits.Length; i++)
            {
                hits[i] = null;
            }

            Physics.OverlapSphereNonAlloc(transform.position, distance, hits);

            return hits.Where(hit => hit);
        }
    }
}
