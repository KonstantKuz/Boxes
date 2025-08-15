using System;
using Gameplay.Interactable.BallInteraction.Abstract;
using Gameplay.Interactable.BallInteraction.Command;
using Gameplay.Interactable.BallInteraction.State;
using Infrastructure.Components;
using Infrastructure.Network.Abstract;
using Mirror;
using Reflex.Attributes;
using UnityEngine;

namespace Gameplay.Interactable.BallInteraction.Components
{
    public class Ball : NetworkBehaviour
    {
        private const float InterpolationSpeed = 20f;

        [SerializeField]
        private BallStateHolder ballStateHolder;

        [SerializeField]
        private Collider collider;

        [SerializeField]
        private GameObject statusEffect;

        private INetworkService networkService;
        private INetworkFactory networkFactory;
        private IBallInteractionMediator ballInteractionMediator;

        private NetworkRigidbodyExtended networkRigidbody;
        private NetworkTransformExtended networkTransform;
        private Transform socketTransform;
        private float distanceSinceLastKick;
        private uint lastKicker;

        private NetworkRigidbodyExtended NetworkRigidbody =>
            networkRigidbody ??= GetComponent<NetworkRigidbodyExtended>();
        private NetworkTransformExtended NetworkTransform =>
            networkTransform ??= GetComponent<NetworkTransformExtended>();

        private Rigidbody Rigidbody => NetworkRigidbody.Rigidbody;
        public Bounds Bounds => collider.bounds;
        private Vector3 Velocity => Rigidbody.velocity;
        private BallInteractionConfig Config => ballInteractionMediator.Config;
        public INetworkStateHolder<BallState> StateHolder => ballStateHolder;

        [Inject]
        private void Construct(
            INetworkService networkService,
            INetworkFactory networkFactory,
            IBallInteractionMediator ballInteractionMediator
        )
        {
            this.networkService = networkService;
            this.networkFactory = networkFactory;
            this.ballInteractionMediator = ballInteractionMediator;
        }

        public override void OnStartServer()
        {
            networkService.ObserveToExecute<KickCommand>(ExecuteKick);
            networkService.ObserveToExecute<CaptureCommand>(ExecuteCapture);
        }

        public override void OnStartClient()
        {
            ballInteractionMediator.RegisterBall(this);
        }

        private void ExecuteKick(KickCommand kickContext)
        {
            NetworkRigidbody.CmdSetEnabled(true);
            NetworkRigidbody.CmdSetIsKinematic(false);

            BallState state = StateHolder.GetState();
            byte kicksCount = (byte) (state.KicksCount + 1);
            kicksCount = (byte) Math.Clamp(kicksCount, 0, Config.MaxKicksCount);

            if (Config.ResetConditions.HasFlag(ResetCondition.SamePlayerKick) &&
                kickContext.InitiatorNetId == lastKicker)
            {
                kicksCount = 1;
            }

            StateHolder.WriteState(new BallState(0, kicksCount));

            float targetSpeed = (1.0f + Config.KickSpeedModifier * kicksCount) * Config.MinSpeed;

            Rigidbody.isKinematic = false;
            Rigidbody.velocity = kickContext.KickDirection.normalized * targetSpeed;

            distanceSinceLastKick = 0;
            lastKicker = kickContext.InitiatorNetId;
        }

        private void ExecuteCapture(CaptureCommand captureContext)
        {
            BallState state = StateHolder.GetState();
            byte kicksCount = Config.ResetConditions.HasFlag(ResetCondition.Capture) ? (byte) 0 : state.KicksCount;
            StateHolder.WriteState(new BallState(captureContext.CaptureRootNetId, kicksCount));

            NetworkRigidbody.CmdSetIsKinematic(true);
            NetworkRigidbody.CmdSetEnabled(false);

            distanceSinceLastKick = 0;
        }

        private void ExecuteResetCounter()
        {
            StateHolder.WriteState(BallState.Default);
            distanceSinceLastKick = 0;
        }

        private void Update()
        {
            BallState state = StateHolder.GetState();

            statusEffect.SetActive(state.KicksCount >= Config.MaxKicksCount);

            if (
                !networkFactory.Spawned.TryGetValue(state.OwnerNetId, out GameObject owner) ||
                !owner.TryGetComponent(out IBallInteractionInitiator ballInteractionInitiator)
            )
            {
                return;
            }

            transform.position = ballInteractionInitiator.BallSocket.position;
        }

        private void FixedUpdate()
        {
            if (!isServer)
            {
                return;
            }

            BallState state = StateHolder.GetState();

            if (state.OwnerNetId != 0)
            {
                return;
            }

            distanceSinceLastKick += Velocity.magnitude * Time.fixedDeltaTime;

            if (transform.position.y > Config.MaxHeight)
            {
                float heightExcess = transform.position.y - Config.MaxHeight;
                float verticalDampingForce = heightExcess * Config.DampingStrength;
                Vector3 velocity = Rigidbody.velocity;
                velocity.y /= Config.DampingStrength;
                Rigidbody.velocity = velocity;
                Rigidbody.AddForce(Vector3.down * verticalDampingForce, ForceMode.Acceleration);
            }

            if (ballInteractionMediator.IsBallOutOfBounds(out Plane outOfBoundsSide))
            {
                Vector3 simplifiedNormal;

                if (Mathf.Abs(outOfBoundsSide.normal.x) > Mathf.Abs(outOfBoundsSide.normal.z))
                {
                    simplifiedNormal = outOfBoundsSide.normal.x > 0 ? Vector3.right : Vector3.left;
                }
                else
                {
                    simplifiedNormal = outOfBoundsSide.normal.z > 0 ? Vector3.forward : Vector3.back;
                }

                if (Vector3.Dot(Velocity, -simplifiedNormal) > 0)
                {
                    Vector3 reflectedVelocity = Vector3.Reflect(Velocity, simplifiedNormal);
                    Rigidbody.velocity = reflectedVelocity;
                }
                else
                {
                    float distanceToPlane = outOfBoundsSide.GetDistanceToPoint(transform.position);
                    Vector3 reboundForce = simplifiedNormal * Mathf.Abs(distanceToPlane) * Config.OutOfBoundsPullForce;
                    Rigidbody.AddForce(reboundForce, ForceMode.Force);
                }
            }

            int kicksCount = StateHolder.GetState().KicksCount;

            if (kicksCount > 0)
            {
                float resetSpeed = (1.0f + Config.KickSpeedModifier * (kicksCount - 1)) * Config.MinSpeed;
                float initialSpeed = (1.0f + Config.KickSpeedModifier * kicksCount) * Config.MinSpeed;

                float currentSpeed = Mathf.Lerp(initialSpeed, resetSpeed, distanceSinceLastKick / Config.DecayDistance);

                Rigidbody.velocity = Velocity.normalized * currentSpeed;

                if (Velocity.magnitude < resetSpeed)
                {
                    ExecuteResetCounter();
                }

                Debug.Log($"Kicks count = {kicksCount}. Velocity = {Velocity.magnitude}. Reset Speed = {resetSpeed}");
            }

            if (Velocity.magnitude > Config.MaxSpeed)
            {
                Rigidbody.velocity = Velocity.normalized * Config.MaxSpeed;
            }
        }
    }
}
