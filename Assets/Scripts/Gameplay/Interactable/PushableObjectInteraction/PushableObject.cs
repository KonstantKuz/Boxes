using System;
using Gameplay.Interactable.PushableObjectInteraction.Abstract;
using Gameplay.Interactable.PushableObjectInteraction.State;
using Infrastructure;
using Infrastructure.Network.Abstract;
using Mirror;
using Reflex.Attributes;
using UnityEngine;

namespace Gameplay.Interactable.PushableObjectInteraction
{
    [RequireComponent(typeof(Rigidbody))]
    public class PushableObject : NetworkBehaviour
    {
        [Header("State")]
        [SerializeField]
        private PushableObjectStateHolder stateHolder;

        [SerializeField]
        private new Rigidbody rigidbody;

        [Header("Physics Parameters")]
        [SerializeField]
        private float kickImpulseMultiplier = 10f;

        private IPushableObjectInteractionMediator mediator;
        private IDisposable stateSubscription;
        private PushableObjectSharedState? previousState;

        public INetworkStateHolder<PushableObjectSharedState> StateHolder => stateHolder;

        [Inject]
        private void Construct(IPushableObjectInteractionMediator mediator)
        {
            this.mediator = mediator;
        }

        private void Awake()
        {
            mediator.RegisterPushableObject(this);
            rigidbody.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
        }

        public override void OnStartClient()
        {
            stateSubscription = StateHolder.Subscribe(OnStateChanged);
        }

        public override void OnStopClient()
        {
            stateSubscription?.Dispose();
        }

        private void OnStateChanged(PushableObjectSharedState newState)
        {
            if (newState.LastActionId != previousState?.LastActionId && newState.LastActionId > 0)
            {
                HandleAction(newState);
            }

            previousState = newState;
        }

        private void HandleAction(PushableObjectSharedState state)
        {
            switch (state.LastActionType)
            {
                case PushableActionType.Kick:
                    ApplyKick(state);
                    break;
            }
        }

        private void ApplyKick(PushableObjectSharedState state)
        {
            Quaternion objectRotation = Quaternion.Euler(0f, transform.eulerAngles.y, 0f);
            Quaternion inverseObjectRotation = Quaternion.Inverse(objectRotation);
            Vector3 localKickDirection = inverseObjectRotation * state.KickDirection;
            Vector3 localKickPosition = inverseObjectRotation * (state.KickPosition - transform.position);

            float absX = Mathf.Abs(localKickDirection.x);
            float absZ = Mathf.Abs(localKickDirection.z);

            if (absX > absZ)
            {
                float torque = localKickPosition.z * localKickDirection.x * state.KickForce * kickImpulseMultiplier;
                Vector3 torqueVector = Vector3.up * torque;
                rigidbody.AddTorque(torqueVector, ForceMode.Impulse);

                this.Log(LogType.Log, $"Kick torque applied: torque={torque}");
            }
            else
            {
                Vector3 localForceDirection = new Vector3(0f, 0f, Mathf.Sign(localKickDirection.z));
                Vector3 worldForce = objectRotation * localForceDirection;
                worldForce *= state.KickForce * kickImpulseMultiplier;
                rigidbody.AddForce(worldForce, ForceMode.Impulse);

                this.Log(LogType.Log, $"Kick force applied: worldForce={worldForce}");
            }
        }

        private void FixedUpdate()
        {
            Vector3 angularVelocity = rigidbody.angularVelocity;
            angularVelocity.x = 0f;
            angularVelocity.z = 0f;
            rigidbody.angularVelocity = angularVelocity;

            Vector3 euler = transform.eulerAngles;
            euler.x = 0f;
            euler.z = 0f;
            transform.eulerAngles = euler;
        }
    }
}
