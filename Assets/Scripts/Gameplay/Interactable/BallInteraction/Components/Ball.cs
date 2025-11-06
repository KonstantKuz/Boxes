using Gameplay.Interactable.BallInteraction.Abstract;
using Gameplay.Interactable.BallInteraction.State;
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

        [SerializeField]
        private LayerMask penetrationTestMask;

        private INetworkManager networkManager;
        private IBallInteractionMediator ballInteractionMediator;

        private BallSharedState? previousState;
        private BallSharedState? pendingAction;
        private float localDistanceSinceLastKick;
        private Vector3 localCaptureRelativePosition;

        public Bounds Bounds => collider.bounds;
        private BallInteractionConfig Config => ballInteractionMediator.Config;
        public INetworkStateHolder<BallSharedState> StateHolder => ballStateHolder;

        [Inject]
        private void Construct(INetworkManager networkManager, IBallInteractionMediator ballInteractionMediator)
        {
            this.networkManager = networkManager;
            this.ballInteractionMediator = ballInteractionMediator;
        }

        private void Awake()
        {
            ballInteractionMediator.RegisterBall(this);
        }

        private void OnEnable()
        {
            rigidbody.isKinematic = collider.isTrigger = false;
        }

        private void OnDisable()
        {
            rigidbody.isKinematic = collider.isTrigger = true;
        }

        public override void OnStartServer()
        {
            StateHolder.Subscribe(OnStateChangedServer);
        }

        public override void OnStartClient()
        {
            StateHolder.Subscribe(OnStateChangedClient);

            if (isServer)
            {
                networkManager.AssignAuthority(netIdentity);
            }
        }

        public override void OnStartAuthority()
        {
            if (pendingAction.HasValue)
            {
                HandleAction(pendingAction.Value);
                pendingAction = null;
            }
        }

        private void OnStateChangedServer(BallSharedState newState)
        {
            if (!isServer)
            {
                return;
            }

            if (newState.OwnerNetId != previousState?.OwnerNetId && newState.OwnerNetId != 0)
            {
                networkManager.AssignAuthority(netIdentity, newState.OwnerNetId);
            }
        }

        private void OnStateChangedClient(BallSharedState newState)
        {
            if (newState.LastActionId != previousState?.LastActionId && newState.LastActionId > 0)
            {
                bool isKinematic = newState.LastActionType is BallActionType.Hold or BallActionType.Capture;

                rigidbody.isKinematic = collider.isTrigger = isKinematic;
                // collider.enabled = !isKinematic;

                IBallInteractionInitiator localInitiator = ballInteractionMediator.LocalInitiator;
                bool isLocal = localInitiator != null && newState.OwnerNetId == localInitiator.NetId;

                if (isOwned && isLocal)
                {
                    HandleAction(newState);
                    pendingAction = null;
                }
                else if (!isOwned && isLocal)
                {
                    pendingAction = newState;
                }
            }

            previousState = newState;
        }

        private void HandleAction(BallSharedState state)
        {
            switch (state.LastActionType)
            {
                case BallActionType.Kick:
                    ApplyKickPhysics(state);
                    break;

                case BallActionType.Hold:
                    ApplyHoldPhysics(state);
                    break;

                case BallActionType.Capture:
                    ApplyCapturePhysics(state);
                    break;
            }
        }

        private void ApplyKickPhysics(BallSharedState state)
        {
            float targetSpeed = (1.0f + Config.KickSpeedModifier * state.KicksCount) * Config.MinSpeed;
            rigidbody.velocity = state.LastKickDirection.normalized * targetSpeed;

            localDistanceSinceLastKick = 0;
            localCaptureRelativePosition = Vector3.zero;
        }

        private void ApplyHoldPhysics(BallSharedState state)
        {
            localDistanceSinceLastKick = 0;
            localCaptureRelativePosition = Vector3.zero;
        }

        private void ApplyCapturePhysics(BallSharedState state)
        {
            if (ballInteractionMediator.Initiators.TryGetValue(state.OwnerNetId, out IBallInteractionInitiator ownerInitiator))
            {
                localCaptureRelativePosition = ownerInitiator.BallSocket.InverseTransformPoint(transform.position);
            }

            localDistanceSinceLastKick = 0;
        }

        private void OnCollisionEnter(Collision other)
        {
            if (!isOwned || other.IsInLayerMask(ignoreCollisionMask))
            {
                return;
            }

            bool isResetRequired = Config.ResetConditions.HasFlag(ResetCondition.Collision);

            IBallReactionInitiator reactionInitiator =
                other.gameObject.GetComponentInParent<IBallReactionInitiator>() ??
                other.gameObject.GetComponent<IBallReactionInitiator>() ??
                other.gameObject.GetComponentInChildren<IBallReactionInitiator>();

            bool hasStatus = StateHolder.GetState().KicksCount >= Config.StatusKicksCount;

            if (hasStatus && reactionInitiator != null && reactionInitiator.TryExecuteReaction())
            {
                isResetRequired = Config.ResetConditions.HasFlag(ResetCondition.Reaction);
            }

            if (isResetRequired)
            {
                ResetCounter();
            }
        }

        private void ResetCounter()
        {
            BallSharedState current = StateHolder.GetState();
            StateHolder.WriteState(new BallSharedState(
                kicksCount: 0,
                ownerNetId: current.OwnerNetId,
                holderNetId: current.HolderNetId,
                lastActionId: current.LastActionId,
                lastActionType: current.LastActionType,
                lastKickDirection: current.LastKickDirection,
                lastKickInitiatorNetId: current.LastKickInitiatorNetId
            ));
            localDistanceSinceLastKick = 0;
        }

        private void Update()
        {
            statusEffect.SetActive(StateHolder.GetState().KicksCount >= Config.StatusKicksCount);
        }

        private void FixedUpdate()
        {
            if (!isOwned)
            {
                return;
            }

            BallSharedState state = StateHolder.GetState();

            if (state.HasHolder)
            {
                if (!ballInteractionMediator.Initiators.TryGetValue(state.HolderNetId, out IBallInteractionInitiator holderInitiator))
                {
                    return;
                }

                Vector3 safePosition = holderInitiator.Position;
                safePosition.y = holderInitiator.BallSocket.position.y;

                Vector3 resultPosition = CollisionExtension.ResolvePenetration(
                    safePosition,
                    holderInitiator.BallSocket.position,
                    collider.bounds.extents.magnitude,
                    penetrationTestMask
                );
                rigidbody.MovePosition(resultPosition);
                return;
            }

            if (localCaptureRelativePosition != Vector3.zero)
            {
                if (!ballInteractionMediator.Initiators.TryGetValue(state.OwnerNetId, out IBallInteractionInitiator ownerInitiator))
                {
                    return;
                }

                Vector3 safePosition = ownerInitiator.Position;
                safePosition.y = ownerInitiator.BallSocket.position.y;

                Vector3 targetPosition =
                    ownerInitiator.BallSocket.TransformPoint(localCaptureRelativePosition);

                Vector3 resultPosition = CollisionExtension.ResolvePenetration(
                    safePosition,
                    targetPosition,
                    collider.bounds.extents.magnitude,
                    penetrationTestMask
                );
                rigidbody.MovePosition(resultPosition);
                return;
            }

            if (transform.position.y > Config.MaxHeight)
            {
                float heightExcess = transform.position.y - Config.MaxHeight;
                float verticalDampingForce = heightExcess * Config.HeightDampingStrength;
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

            rigidbody.velocity = Vector3.ClampMagnitude(rigidbody.velocity, Config.MaxSpeed);

            localDistanceSinceLastKick += rigidbody.velocity.magnitude * Time.fixedDeltaTime;

            int kicksCount = StateHolder.GetState().KicksCount;

            if (kicksCount > 0)
            {
                float resetSpeed = (1.0f + Config.KickSpeedModifier * (kicksCount - 1)) * Config.MinSpeed;
                float initialSpeed = (1.0f + Config.KickSpeedModifier * kicksCount) * Config.MinSpeed;

                float currentSpeed =
                    Mathf.Lerp(initialSpeed, resetSpeed, localDistanceSinceLastKick / Config.StatusDampingDistance);

                rigidbody.velocity = rigidbody.velocity.normalized * currentSpeed;

                if (rigidbody.velocity.magnitude < resetSpeed)
                {
                    ResetCounter();
                }
            }
        }
    }
}
