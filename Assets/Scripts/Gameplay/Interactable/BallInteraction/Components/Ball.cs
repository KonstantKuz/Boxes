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
        private BallInteractionConfig config;

        [SerializeField]
        private BallStateHolder ballStateHolder;

        [SerializeField]
        private Collider collider;

        private INetworkService networkService;
        private INetworkFactory networkFactory;
        private IBallInteractionMediator ballInteractionMediator;

        private NetworkRigidbodyExtended networkRigidbody;
        private NetworkTransformExtended networkTransform;
        private Transform socketTransform;
        private int kicksCount;
        private float distanceSinceLastKick;

        private NetworkRigidbodyExtended NetworkRigidbody =>
            networkRigidbody ??= GetComponent<NetworkRigidbodyExtended>();

        private NetworkTransformExtended NetworkTransform =>
            networkTransform ??= GetComponent<NetworkTransformExtended>();

        private Rigidbody Rigidbody => NetworkRigidbody.Rigidbody;

        private INetworkStateHolder<BallState> BallStateHolder => ballStateHolder;

        public Bounds Bounds => collider.bounds;

        private Vector3 Velocity => Rigidbody.velocity;

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
            networkService.ObserveToExecute<KickCommand>(CmdHandleKick);
            networkService.ObserveToExecute<CaptureCommand>(CmdHandleCapture);
        }

        public override void OnStartClient()
        {
            ballInteractionMediator.RegisterBall(this);
        }

        public void DisplayDirection(Vector2 direction)
        {

        }

        private void CmdHandleKick(KickCommand kickContext)
        {
            BallStateHolder.WriteState(BallState.Default);

            NetworkRigidbody.CmdSetEnabled(true);
            NetworkRigidbody.CmdSetIsKinematic(false);

            kicksCount++;

            kicksCount = Math.Clamp(kicksCount, 0, config.MaxKicksCount);

            float targetSpeed = (1.0f + config.KickSpeedModifier * kicksCount) * config.MinSpeed;

            Rigidbody.velocity = kickContext.KickDirection.normalized * targetSpeed;
            distanceSinceLastKick = 0;
            Debug.Log("ball kicked");
        }

        private void CmdHandleCapture(CaptureCommand captureContext)
        {
            BallStateHolder.WriteState(new BallState(captureContext.CaptureRootNetId));

            NetworkRigidbody.CmdSetIsKinematic(true);
            NetworkRigidbody.CmdSetEnabled(false);

            kicksCount = 0;
            distanceSinceLastKick = 0;
        }

        private void Update()
        {
            if (!isServer)
            {
                return;
            }

            uint ownerNetId = BallStateHolder.GetState()?.OwnerNetId ?? 0;

            if (
                !networkFactory.Spawned.TryGetValue(ownerNetId, out GameObject owner) ||
                !owner.TryGetComponent(out IBallInteractionInitiator ballInteractionInitiator)
            )
            {
                return;
            }

            // transform.position = Vector3.Lerp(
            //     transform.position, ballInteractionInitiator.BallSocket.position, InterpolationSpeed * Time.deltaTime
            // );

            transform.position = ballInteractionInitiator.BallSocket.position;
        }

        private void FixedUpdate()
        {
            if (!isServer)
            {
                return;
            }

            distanceSinceLastKick += Velocity.magnitude * Time.fixedDeltaTime;

            if (transform.position.y > config.MaxHeight)
            {
                float heightExcess = transform.position.y - config.MaxHeight;
                float verticalDampingForce = heightExcess * config.DampingStrength;
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
                    Vector3 reboundForce = simplifiedNormal * Mathf.Abs(distanceToPlane) * config.OutOfBoundsPullForce;
                    Rigidbody.AddForce(reboundForce, ForceMode.Force);
                }

                // Vector3 projectedNormal = Vector3.ProjectOnPlane(outOfBoundsSide.normal, Vector3.up).normalized;
                // if (Vector3.Dot(velocity, -projectedNormal) > 0)
                // {
                //     Vector3 reflectedVelocity = Vector3.Reflect(velocity, projectedNormal);
                //     Rigidbody.velocity = reflectedVelocity;
                // }
                // else
                // {
                //     float distanceToPlane = outOfBoundsSide.GetDistanceToPoint(transform.position);
                //     Vector3 reboundForce = -projectedNormal * Mathf.Abs(distanceToPlane) * config.OutOfBoundsPullForce;
                //     Rigidbody.AddForce(reboundForce, ForceMode.Force);
                // }
            }

            if (kicksCount > 0)
            {
                float resetSpeed = (1.0f + config.KickSpeedModifier * (kicksCount - 1)) * config.MinSpeed;
                float initialSpeed = (1.0f + config.KickSpeedModifier * kicksCount) * config.MinSpeed;

                float currentSpeed = Mathf.Lerp(initialSpeed, resetSpeed, distanceSinceLastKick / config.DecayDistance);

                Rigidbody.velocity = Velocity.normalized * currentSpeed;

                if (Velocity.magnitude < resetSpeed)
                {
                    kicksCount = 0;
                }

                Debug.Log($"Kicks count = {kicksCount}. Velocity = {Velocity.magnitude}. Reset Speed = {resetSpeed}");
            }

            if (Velocity.magnitude > config.MaxSpeed)
            {
                Rigidbody.velocity = Velocity.normalized * config.MaxSpeed;
            }
        }
    }
}
