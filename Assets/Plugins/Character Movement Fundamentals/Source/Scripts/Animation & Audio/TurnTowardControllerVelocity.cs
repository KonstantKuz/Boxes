using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CMF
{
	//This script turns a gameobject toward the target controller's velocity direction;
	public class TurnTowardControllerVelocity : MonoBehaviour {

		//Target controller;
		public Controller controller;

		//Speed at which this gameobject turns toward the controller's velocity;
		public float turnSpeed = 500f;

		private Transform cameraTransform;

		//Current (local) rotation around the (local) y axis of this gameobject;
		float currentYRotation = 0f;

		//If the angle between the current and target direction falls below 'fallOffAngle', 'turnSpeed' becomes progressively slower (and eventually approaches '0f');
		//This adds a smoothing effect to the rotation;
		float fallOffAngle = 90f;

		//Whether the current controller momentum should be ignored when calculating the new direction;
		public bool ignoreControllerMomentum = false;

		private Vector3 targetDirection;

		//Setup;
		void Start () {
			//Throw warning if no controller has been assigned;
			if(controller == null)
			{
				Debug.LogWarning("No controller script has been assigned to this 'TurnTowardControllerVelocity' component!", this);
				this.enabled = false;
			}
		}

		public void SetCameraTransform(Transform cameraTransform)
		{
			this.cameraTransform = cameraTransform;
		}

		public void SetTargetDirection(Vector3 targetDirection)
		{
			if (cameraTransform == null)
			{
				return;
			}

			Vector3 result = Vector3.zero;
			result += Vector3.ProjectOnPlane(cameraTransform.right, transform.up).normalized * targetDirection.x;
			result += Vector3.ProjectOnPlane(cameraTransform.forward, transform.up).normalized * targetDirection.y;
			this.targetDirection = result;
		}

		void LateUpdate () {

			//Get controller velocity;
			Vector3 _velocity;
			if(ignoreControllerMomentum)
				_velocity = controller.GetMovementVelocity();
			else
				_velocity = controller.GetVelocity();

			if (targetDirection.sqrMagnitude > 0)
			{
				_velocity = targetDirection;
			}

			//Project velocity onto a plane defined by the 'up' direction of the parent transform;
			_velocity = Vector3.ProjectOnPlane(_velocity, transform.parent.up);

			float _magnitudeThreshold = 0.001f;

			//If the velocity's magnitude is smaller than the threshold, return;
			if(_velocity.magnitude < _magnitudeThreshold)
				return;

			//Normalize velocity direction;
			_velocity.Normalize();

			//Get current 'forward' vector;
			Vector3 _currentForward = transform.forward;

			//Calculate (signed) angle between velocity and forward direction;
			float _angleDifference = VectorMath.GetAngle(_currentForward, _velocity, transform.parent.up);

			//Calculate angle factor;
			float _factor = Mathf.InverseLerp(0f, fallOffAngle, Mathf.Abs(_angleDifference));

			//Calculate this frame's step;
			float _step = Mathf.Sign(_angleDifference) * _factor * Time.deltaTime * turnSpeed;

			//Clamp step;
			if(_angleDifference < 0f && _step < _angleDifference)
				_step = _angleDifference;
			else if(_angleDifference > 0f && _step > _angleDifference)
				_step = _angleDifference;

			//Add step to current y angle;
			currentYRotation += _step;

			//Clamp y angle;
			if(currentYRotation > 360f)
				currentYRotation -= 360f;
			if(currentYRotation < -360f)
				currentYRotation += 360f;

			//Set transform rotation using Quaternion.Euler;
			transform.localRotation = Quaternion.Euler(0f, currentYRotation, 0f);

		}

		void OnDisable()
		{
		}

		void OnEnable()
		{
			currentYRotation = transform.localEulerAngles.y;
		}
	}
}
