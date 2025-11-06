using DG.Tweening;
using UnityEngine;
using UnityEngine.AI;

namespace Gameplay.Components
{
    public class RotateTowardsMovement : MonoBehaviour
    {
        private const float SmoothSpeed = 10f;

        private Vector3 previousPosition;
        private Vector3 targetDirection;

        private void Awake()
        {
            if (TryGetComponent(out NavMeshAgent agent))
            {
                agent.updateRotation = false;
            }
        }

        public void RotateTowards(Vector3 targetDirection)
        {
            this.targetDirection = targetDirection;
            transform.DORotateQuaternion(Quaternion.LookRotation(targetDirection), 0.5f);
        }

        private void Update()
        {
            Vector3 direction = transform.position - previousPosition;
            if (targetDirection.magnitude > Mathf.Epsilon)
            {
                direction = targetDirection;
                targetDirection = Vector3.zero;
            }
            if (direction.magnitude > Mathf.Epsilon)
            {
                transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(direction), Time.deltaTime * SmoothSpeed);
            }
            previousPosition = transform.position;
        }
    }
}
