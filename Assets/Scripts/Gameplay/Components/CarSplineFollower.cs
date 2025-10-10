using Sirenix.OdinInspector;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Splines;

namespace Gameplay.Components
{
    [RequireComponent(typeof(Rigidbody))]
    public class CarSplineFollower : MonoBehaviour
    {
        [SerializeField]
        private SplineContainer splineContainer;

        [SerializeField]
        private bool loop = false;

        [SerializeField]
        private float speed = 10f;

        [SerializeField]
        private float minSpeed = 4f;

        [SerializeField]
        private float startDistance = 0f;

        [SerializeField]
        private float frontAxisOffset = 2f;

        [SerializeField]
        private float rearAxisOffset = 2f;

        [SerializeField]
        private float rotationSmoothing = 8f;

        [SerializeField]
        private float turnPredictionDistance = 5f;

        [SerializeField]
        private float maxTurnAngle = 45f;

        [SerializeField]
        private float accelerationSmoothing = 4f;

        private Rigidbody rigidbodyComponent;
        private float splineLength;
        private float distanceAlong;
        private float currentSpeed;
        private Quaternion currentRotation;
        private Vector3 frontAxisPosition;
        private Vector3 rearAxisPosition;

        private void Start()
        {
            rigidbodyComponent = GetComponent<Rigidbody>();

            if (splineContainer == null)
            {
                enabled = false;
                return;
            }

            splineLength = splineContainer.CalculateLength();
            distanceAlong = startDistance;
            currentRotation = transform.rotation;
            currentSpeed = speed;
        }

        private void FixedUpdate()
        {
            if (splineContainer == null || !enabled)
            {
                return;
            }

            if (splineLength <= 0f)
            {
                splineLength = splineContainer.CalculateLength();
            }

            float turnFactor = CalculateTurnFactor();
            float targetSpeed = Mathf.Lerp(speed, minSpeed, turnFactor);
            currentSpeed = Mathf.Lerp(currentSpeed, targetSpeed, 1f - Mathf.Exp(-accelerationSmoothing * Time.fixedDeltaTime));

            distanceAlong += currentSpeed * Time.fixedDeltaTime;

            if (loop)
            {
                distanceAlong %= splineLength;
                if (distanceAlong < 0f)
                {
                    distanceAlong += splineLength;
                }
            }
            else
            {
                distanceAlong = Mathf.Clamp(distanceAlong, 0f, splineLength);
            }

            float normalizedFront = DistanceToNormalizedT(distanceAlong + frontAxisOffset);
            float normalizedRear = DistanceToNormalizedT(distanceAlong - rearAxisOffset);

            frontAxisPosition = splineContainer.EvaluatePosition(normalizedFront);
            rearAxisPosition = splineContainer.EvaluatePosition(normalizedRear);

            Vector3 direction = (frontAxisPosition - rearAxisPosition).normalized;
            Quaternion targetRotation = Quaternion.LookRotation(direction, Vector3.up);
            currentRotation = Quaternion.Slerp(currentRotation, targetRotation, 1f - Mathf.Exp(-rotationSmoothing * Time.fixedDeltaTime));

            rigidbodyComponent.MovePosition(frontAxisPosition - direction * frontAxisOffset * 0.5f);
            rigidbodyComponent.MoveRotation(currentRotation);
        }

        private float CalculateTurnFactor()
        {
            float normalizedCurrent = DistanceToNormalizedT(distanceAlong);
            float normalizedAhead = DistanceToNormalizedT(distanceAlong + turnPredictionDistance);

            Vector3 tangentCurrent = splineContainer.EvaluateTangent(normalizedCurrent);
            Vector3 tangentAhead = splineContainer.EvaluateTangent(normalizedAhead);

            float angle = Vector3.Angle(tangentCurrent, tangentAhead);
            float factor = Mathf.Clamp01(angle / maxTurnAngle);
            return factor;
        }

        private float DistanceToNormalizedT(float distance)
        {
            if (splineLength <= 0f)
            {
                splineLength = splineContainer.CalculateLength();
            }

            if (splineLength <= 0f)
            {
                return 0f;
            }

            float distanceValue = distance;

            if (loop)
            {
                distanceValue = distanceValue % splineLength;
                if (distanceValue < 0f)
                {
                    distanceValue += splineLength;
                }
            }
            else
            {
                distanceValue = Mathf.Clamp(distanceValue, 0f, splineLength);
            }

            return Mathf.Clamp01(distanceValue / splineLength);
        }
        [Button]
        private void ProjectStartPosition()
        {
            if (splineContainer == null)
            {
                Debug.LogWarning("No spline.");
                return;
            }

            float3 local = splineContainer.transform.InverseTransformPoint(transform.position);
            Spline spline = splineContainer.Spline;
            SplineUtility.GetNearestPoint(spline, local, out float3 nearestLocal, out float t);
            float3 tangentLocal = spline.EvaluateTangent(t);
            Vector3 worldTangent = splineContainer.transform.TransformDirection(tangentLocal);

            splineLength = splineContainer.CalculateLength();
            startDistance = t * splineLength;

            transform.position = splineContainer.transform.TransformPoint(nearestLocal);
            transform.rotation = Quaternion.LookRotation(worldTangent, Vector3.up);
        }

        private void OnDrawGizmos()
        {
            if (splineContainer == null)
            {
                return;
            }

            Gizmos.color = Color.green;
            Gizmos.DrawSphere(frontAxisPosition, 0.2f);

            Gizmos.color = Color.red;
            Gizmos.DrawSphere(rearAxisPosition, 0.2f);

            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(rearAxisPosition, frontAxisPosition);

            Gizmos.color = Color.blue;
            Gizmos.DrawSphere(splineContainer.EvaluatePosition(startDistance), 0.2f);
        }
    }
}
