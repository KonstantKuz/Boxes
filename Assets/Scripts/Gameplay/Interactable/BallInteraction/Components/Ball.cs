using System;
using Gameplay.Interactable.BallInteraction.Abstract;
using Gameplay.Interactable.BallInteraction.Command;
using Gameplay.Interactable.BallInteraction.State;
using Infrastructure.Components;
using Infrastructure.Extensions;
using Infrastructure.Network.Abstract;
using Mirror;
using Reflex.Attributes;
using UnityEngine;

namespace Gameplay.Interactable.BallInteraction.Components
{
    public class Ball : NetworkBehaviour
    {
        [SerializeField]
        private BallStateHolder ballStateHolder;

        [SerializeField]
        private new Collider collider;

        [SerializeField]
        private new Rigidbody rigidbody;

        [SerializeField]
        private GameObject statusEffect;

        [SerializeField]
        private LayerMask ignoreCollisionMask;

        private INetworkService networkService;
        private INetworkFactory networkFactory;
        private IBallInteractionMediator ballInteractionMediator;

        private NetworkRigidbodyExtended networkRigidbody;
        private NetworkTransformExtended networkTransform;
        private Transform socketTransform;
        private BallInternalState internalState;

        public Bounds Bounds => collider.bounds;
        private BallInteractionConfig Config => ballInteractionMediator.Config;
        public INetworkStateHolder<BallSharedState> StateHolder => ballStateHolder;

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

            internalState = new BallInternalState();
        }

        public override void OnStartServer()
        {
            networkService.ObserveToExecute<KickCommand>(ExecuteKick);
            networkService.ObserveToExecute<HoldCommand>(ExecuteHold);
            networkService.ObserveToExecute<CaptureCommand>(ExecuteCapture);
        }

        public override void OnStartClient()
        {
            ballInteractionMediator.RegisterBall(this);
        }

        private void ExecuteKick(KickCommand kickContext)
        {
            BallSharedState state = StateHolder.GetState();
            byte kicksCount = (byte) (state.KicksCount + 1);
            kicksCount = (byte) Math.Clamp(kicksCount, 0, Config.StatusKicksCount);

            if (Config.ResetConditions.HasFlag(ResetCondition.SamePlayerKick) &&
                kickContext.InitiatorNetId == internalState.LastKickerNetId)
            {
                kicksCount = 1;
            }

            StateHolder.WriteState(new BallSharedState(0, kicksCount));

            float targetSpeed = (1.0f + Config.KickSpeedModifier * kicksCount) * Config.MinSpeed;

            rigidbody.isKinematic = false;
            rigidbody.velocity = kickContext.Direction.normalized * targetSpeed;

            internalState.DistanceSinceLastKick = 0;
            internalState.LastKickerNetId = kickContext.InitiatorNetId;
            internalState.CaptureContext = null;
        }

        private void ExecuteHold(HoldCommand holdContext)
        {
            BallSharedState state = StateHolder.GetState();
            byte kicksCount = Config.ResetConditions.HasFlag(ResetCondition.Hold) ? (byte) 0 : state.KicksCount;
            StateHolder.WriteState(new BallSharedState(holdContext.InitiatorNetId, kicksCount));

            internalState.DistanceSinceLastKick = 0;
            internalState.CaptureContext = null;
        }

        private void ExecuteCapture(CaptureCommand captureContext)
        {
            internalState.CaptureContext = captureContext;
            internalState.DistanceSinceLastKick = 0;
        }

        private void ExecuteResetCounter()
        {
            StateHolder.WriteState(BallSharedState.Default);
            internalState.DistanceSinceLastKick = 0;
        }

        private void OnCollisionEnter(Collision other)
        {
            if (!isServer || other.IsInLayerMask(ignoreCollisionMask))
            {
                return;
            }

            bool isResetRequired = Config.ResetConditions.HasFlag(ResetCondition.Collision);

            if (other.gameObject.TryGetComponent(out IBallReactionInitiator reactionInitiator))
            {
                BallSharedState state = StateHolder.GetState();
                reactionInitiator.TryExecuteReaction(other, state );
                isResetRequired = state.KicksCount >= Config.StatusKicksCount &&
                                  Config.ResetConditions.HasFlag(ResetCondition.Reaction);
            }

            if (isResetRequired)
            {
                ExecuteResetCounter();
            }
        }

        private void Update()
        {
            BallSharedState state = StateHolder.GetState();

            statusEffect.SetActive(state.KicksCount >= Config.StatusKicksCount);

            rigidbody.isKinematic = state.HasHolder || internalState?.CaptureContext?.InitiatorNetId > 0;
        }

        private void FixedUpdate()
        {
            if (!isServer)
            {
                return;
            }

            BallSharedState state = StateHolder.GetState();

            bool hasValidHolder = ballInteractionMediator.Initiators.TryGetValue(
                state.HolderNetId, out IBallInteractionInitiator holdInitiator
            );

            if (hasValidHolder)
            {
                rigidbody.MovePosition(holdInitiator.BallSocket.position);
                return;
            }

            bool hasValidCaptureTarget = ballInteractionMediator.Initiators.TryGetValue(
                internalState.CaptureContext?.InitiatorNetId ?? 0, out IBallInteractionInitiator captureInitiator
            );

            if (hasValidCaptureTarget)
            {
                Vector3 targetPosition =
                    captureInitiator.BallSocket.TransformPoint(internalState.CaptureContext!.RelativePosition);
                rigidbody.MovePosition(targetPosition);
                return;
            }

            internalState.DistanceSinceLastKick += rigidbody.velocity.magnitude * Time.fixedDeltaTime;

            if (transform.position.y > Config.MaxHeight)
            {
                float heightExcess = transform.position.y - Config.MaxHeight;
                float verticalDampingForce = heightExcess * Config.HeightDampingStrength;
                Vector3 velocity = rigidbody.velocity;
                velocity.y /= Config.HeightDampingStrength;
                rigidbody.velocity = velocity;
                rigidbody.AddForce(Vector3.down * verticalDampingForce, ForceMode.Acceleration);
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

                if (Vector3.Dot(rigidbody.velocity, -simplifiedNormal) > 0)
                {
                    Vector3 reflectedVelocity = Vector3.Reflect(rigidbody.velocity, simplifiedNormal);
                    rigidbody.velocity = reflectedVelocity;
                }
                else
                {
                    float distanceToPlane = outOfBoundsSide.GetDistanceToPoint(transform.position);
                    Vector3 reboundForce = simplifiedNormal * Mathf.Abs(distanceToPlane) * Config.OutOfBoundsPullForce;
                    rigidbody.AddForce(reboundForce, ForceMode.Force);
                }
            }

            int kicksCount = StateHolder.GetState().KicksCount;

            if (kicksCount > 0)
            {
                float resetSpeed = (1.0f + Config.KickSpeedModifier * (kicksCount - 1)) * Config.MinSpeed;
                float initialSpeed = (1.0f + Config.KickSpeedModifier * kicksCount) * Config.MinSpeed;

                float currentSpeed =
                    Mathf.Lerp(initialSpeed, resetSpeed, internalState.DistanceSinceLastKick / Config.StatusDampingDistance);

                rigidbody.velocity = rigidbody.velocity.normalized * currentSpeed;

                if (rigidbody.velocity.magnitude < resetSpeed)
                {
                    ExecuteResetCounter();
                }

                Debug.Log($"Kicks count = {kicksCount}. " +
                          $"Velocity = {rigidbody.velocity.magnitude}." +
                          $" Reset Speed = {resetSpeed}");
            }

            if (rigidbody.velocity.magnitude > Config.MaxSpeed)
            {
                rigidbody.velocity = rigidbody.velocity.normalized * Config.MaxSpeed;
            }

            if (rigidbody.velocity.magnitude > Config.AttractionMinRequiredSpeed)
            {
                float minDistance = float.MaxValue;
                IBallInteractionInitiator nearestInitiator = null;

                foreach (IBallInteractionInitiator initiator in ballInteractionMediator.Initiators.Values)
                {
                    if (initiator.NetId == internalState.LastKickerNetId)
                    {
                        continue;
                    }

                    float distance = Vector3.Distance(transform.position, initiator.BallSocket.position);
                    if (distance <= Config.AttractionRadius && distance < minDistance)
                    {
                        minDistance = distance;
                        nearestInitiator = initiator;
                    }
                }

                if (nearestInitiator != null)
                {
                    Vector3 direction = (nearestInitiator.BallSocket.position - transform.position).normalized;
                    rigidbody.AddForce(direction * Config.AttractionForce, ForceMode.Acceleration);
                }
            }
        }
    }
}
