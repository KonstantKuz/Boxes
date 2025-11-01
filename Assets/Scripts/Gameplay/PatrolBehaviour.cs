using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Gameplay.Components;
using UnityEngine;
using UnityEngine.AI;

namespace Gameplay
{
    public class PatrolBehaviour : MonoBehaviour
    {
        [SerializeField]
        private float waitTime = 2f;

        [SerializeField]
        private NavMeshAgent navMeshAgent;

        [SerializeField]
        private List<Transform> patrolPoints;

        private int currentPointIndex = 0;

        private void OnEnable()
        {
            if (patrolPoints == null || patrolPoints.Count == 0)
            {
                Debug.LogWarning("PatrolBehaviour: no patrol points assigned");
                return;
            }

            StartCoroutine(PatrolRoutine());
        }

        private void OnDisable()
        {
            StopAllCoroutines();
        }

        private IEnumerator PatrolRoutine()
        {
            currentPointIndex = patrolPoints
                .OrderBy(item => Vector3.Distance(item.position, transform.position))
                .Select((_, index) => index)
                .First();

            while (true)
            {
                Transform targetPoint = patrolPoints[currentPointIndex];
                navMeshAgent.SetDestination(targetPoint.position);

                yield return null;

                while (navMeshAgent.remainingDistance > navMeshAgent.stoppingDistance)
                {
                    yield return null;
                }

                if (navMeshAgent.TryGetComponent(out RotateTowardsMovement rotation))
                {
                    rotation.RotateTowards(targetPoint.forward);
                }

                yield return new WaitForSeconds(waitTime);

                currentPointIndex = (currentPointIndex + 1) % patrolPoints.Count;
            }
        }
    }
}
