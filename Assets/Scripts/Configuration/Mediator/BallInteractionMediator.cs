using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Gameplay.Interactable.BallInteraction;
using Gameplay.Interactable.BallInteraction.Abstract;
using Gameplay.Interactable.BallInteraction.Components;
using Gameplay.Interactable.BallInteraction.State;
using Infrastructure.Bootstrap;
using Infrastructure.CameraService;
using Infrastructure.InputService.Abstract;
using Infrastructure.Network.Abstract;
using R3;
using Reflex.Attributes;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Configuration.Mediator
{
    [Serializable]
    public partial class BallInteractionMediator : IBallInteractionMediator, IInitializable, IUpdatable
    {
        [SerializeField]
        private BallInteractionConfig ballInteractionConfig;

        private IInputService inputService;
        private ICameraService cameraService;
        private INetworkService networkService;
        private ReactiveProperty<BallSharedState> stateReactive;
        private Dictionary<uint, IBallInteractionInitiator> initiators;

        private Ball ball;
        private IBallInteractionInitiator localInitiator;
        private CancellationTokenSource kickTokenSource;
        private CancellationTokenSource captureTokenSource;
        private CancellationTokenSource holdTokenSource;
        private float lastKickTime;

        Ball IBallInteractionMediator.Ball => ball;
        BallInteractionConfig IBallInteractionMediator.Config => ballInteractionConfig;
        ReadOnlyReactiveProperty<BallSharedState> IBallInteractionMediator.BallState => stateReactive;

        IReadOnlyDictionary<uint, IBallInteractionInitiator> IBallInteractionMediator.Initiators => initiators;
        IBallInteractionInitiator IBallInteractionMediator.LocalInitiator => localInitiator;


        [Inject]
        private void Construct(IInputService inputService, ICameraService cameraService, INetworkService networkService)
        {
            this.inputService = inputService;
            this.cameraService = cameraService;
            this.networkService = networkService;

            stateReactive = new ReactiveProperty<BallSharedState>(BallSharedState.Default);
            initiators = new Dictionary<uint, IBallInteractionInitiator>();
        }

        void IInitializable.Initialize()
        {
            inputService.DefaultContextActions.Take.performed += TryHoldOrReleaseBall;
            inputService.DefaultContextActions.Action.performed += TryKickBall;
        }

        void IBallInteractionMediator.RegisterBall(Ball ball)
        {
            this.ball = ball;
            this.ball.StateHolder.Subscribe(value => stateReactive.Value = value);
        }

        void IBallInteractionMediator.RegisterInitiator(IBallInteractionInitiator initiator, bool isLocalPlayer)
        {
            if (isLocalPlayer)
            {
                localInitiator = initiator;
            }

            initiators.Add(initiator.NetId, initiator);
        }

        bool IBallInteractionMediator.IsBallOutOfBounds(out Plane outOfBoundsSide)
        {
            outOfBoundsSide = default;
            return ball != null && !cameraService.IsVisible(ball.Bounds, out outOfBoundsSide);
        }

        void IUpdatable.Update()
        {
            if (inputService.DefaultContextActions.Aim.IsPressed())
            {
                TryCaptureBall();
            }
        }

        bool IBallInteractionMediator.IsPredictionVisible(out Vector3 direction)
        {
            direction = Vector3.zero;

            if (localInitiator == null)
            {
                return false;
            }

            direction = localInitiator.KickDirection;

            float distance = Vector3.Distance(localInitiator.Position, ball.transform.position);
            bool isInRange = distance <= ballInteractionConfig.InteractionDistance;

            return isInRange && inputService.DefaultContextActions.Aim.IsPressed();
        }

        private void TryHoldOrReleaseBall(InputAction.CallbackContext context)
        {
            if (ball == null || localInitiator == null || holdTokenSource != null)
            {
                return;
            }

            BallSharedState currentState = ball.StateHolder.GetState();
            if (currentState.HasHolder && currentState.HolderNetId == localInitiator.NetId)
            {
                ball.StateHolder.WriteState(new BallSharedState(
                    kicksCount: currentState.KicksCount,
                    ownerNetId: localInitiator.NetId,
                    holderNetId: 0,
                    lastActionId: currentState.LastActionId + 1,
                    lastActionType: BallActionType.Release,
                    lastKickDirection: currentState.LastKickDirection,
                    lastKickInitiatorNetId: currentState.LastKickInitiatorNetId
                ));
                return;
            }

            holdTokenSource = new CancellationTokenSource();

            TryHoldBallAsync(holdTokenSource.Token).Forget();

            async UniTask TryHoldBallAsync(CancellationToken token)
            {
                float time = 0;

                while (!token.IsCancellationRequested && time < ballInteractionConfig.HoldWindowTime)
                {
                    time += Time.fixedDeltaTime;

                    float distance = (localInitiator.Position - ball.transform.position).magnitude;

                    if (distance <= ballInteractionConfig.InteractionDistance)
                    {
                        holdTokenSource = null;
                        captureTokenSource?.Cancel();
                        captureTokenSource = null;
                        kickTokenSource?.Cancel();
                        kickTokenSource = null;

                        BallSharedState current = ball.StateHolder.GetState();
                        byte kicksCount = ballInteractionConfig.ResetConditions.HasFlag(ResetCondition.Hold)
                            ? (byte)0
                            : current.KicksCount;

                        networkService.AssignAuthority(ball.netId, localInitiator.NetId);

                        ball.StateHolder.WriteState(new BallSharedState(
                            kicksCount: kicksCount,
                            ownerNetId: localInitiator.NetId,
                            holderNetId: localInitiator.NetId,
                            lastActionId: current.LastActionId + 1,
                            lastActionType: BallActionType.Hold,
                            lastKickDirection: Vector3.zero,
                            lastKickInitiatorNetId: current.LastKickInitiatorNetId
                        ));
                        break;
                    }

                    await UniTask.WaitForFixedUpdate();
                }

                holdTokenSource = null;
            }
        }

        private void TryKickBall(InputAction.CallbackContext context)
        {
            if (ball == null || localInitiator == null || kickTokenSource != null)
            {
                return;
            }

            kickTokenSource = new CancellationTokenSource();

            TryKickBallAsync(kickTokenSource.Token).Forget();

            async UniTask TryKickBallAsync(CancellationToken token)
            {
                float time = 0;

                while (!token.IsCancellationRequested && time < ballInteractionConfig.KickWindowTime)
                {
                    time += Time.fixedDeltaTime;

                    float distance = (localInitiator.Position - ball.transform.position).magnitude;

                    if (distance <= ballInteractionConfig.InteractionDistance)
                    {
                        kickTokenSource = null;
                        captureTokenSource?.Cancel();
                        captureTokenSource = null;
                        lastKickTime = Time.time;

                        BallSharedState current = ball.StateHolder.GetState();

                        byte kicksCount = (byte)(current.KicksCount + 1);
                        kicksCount = (byte)Math.Clamp(kicksCount, 0, ballInteractionConfig.StatusKicksCount);

                        if (ballInteractionConfig.ResetConditions.HasFlag(ResetCondition.SamePlayerKick) &&
                            localInitiator.NetId == current.LastKickInitiatorNetId)
                        {
                            kicksCount = 1;
                        }

                        networkService.AssignAuthority(ball.netId, localInitiator.NetId);

                        ball.StateHolder.WriteState(new BallSharedState(
                            kicksCount: kicksCount,
                            ownerNetId: localInitiator.NetId,
                            holderNetId: 0,
                            lastActionId: current.LastActionId + 1,
                            lastActionType: BallActionType.Kick,
                            lastKickDirection: localInitiator.KickDirection,
                            lastKickInitiatorNetId: localInitiator.NetId
                        ));
                        break;
                    }

                    await UniTask.WaitForFixedUpdate();
                }

                kickTokenSource = null;
            }
        }

        private void TryCaptureBall()
        {
            if (ballInteractionConfig.AutoCaptureTime <= 0)
            {
                return;
            }

            float lastKickDeltaTime = Time.time - lastKickTime;
            bool isRecharged = lastKickDeltaTime > ballInteractionConfig.AutoCaptureRechargeTime;

            if (ball == null || localInitiator == null || captureTokenSource != null
                || kickTokenSource != null || !isRecharged || ball.StateHolder.GetState().HasHolder)
            {
                return;
            }

            float distance = (localInitiator.Position - ball.transform.position).magnitude;

            if (distance <= ballInteractionConfig.InteractionDistance)
            {
                captureTokenSource = new CancellationTokenSource();

                TryCaptureBallAsync(captureTokenSource.Token).Forget();
            }

            async UniTask TryCaptureBallAsync(CancellationToken token)
            {
                if (token.IsCancellationRequested)
                {
                    return;
                }

                networkService.AssignAuthority(ball.netId, localInitiator.NetId);

                BallSharedState current = ball.StateHolder.GetState();
                ball.StateHolder.WriteState(new BallSharedState(
                    kicksCount: current.KicksCount,
                    ownerNetId: localInitiator.NetId,
                    holderNetId: 0,
                    lastActionId: current.LastActionId + 1,
                    lastActionType: BallActionType.Capture,
                    lastKickDirection: localInitiator.BallSocket.InverseTransformPoint(ball.transform.position),
                    lastKickInitiatorNetId: current.LastKickInitiatorNetId
                ));

                while (!ball.isOwned && !token.IsCancellationRequested)
                {
                    await UniTask.WaitForFixedUpdate();
                }

                await UniTask.WaitForSeconds(
                    ballInteractionConfig.AutoCaptureTime,
                    delayTiming: PlayerLoopTiming.FixedUpdate,
                    cancellationToken: token
                );

                if (token.IsCancellationRequested)
                {
                    return;
                }

                current = ball.StateHolder.GetState();

                byte kicksCount = (byte)(current.KicksCount + 1);
                kicksCount = (byte)Math.Clamp(kicksCount, 0, ballInteractionConfig.StatusKicksCount);

                if (ballInteractionConfig.ResetConditions.HasFlag(ResetCondition.SamePlayerKick) &&
                    localInitiator.NetId == current.LastKickInitiatorNetId)
                {
                    kicksCount = 1;
                }

                uint actionId = ball.StateHolder.GetState().LastActionId + 1;

                ball.StateHolder.WriteState(new BallSharedState(
                    kicksCount: kicksCount,
                    ownerNetId: localInitiator.NetId,
                    holderNetId: 0,
                    lastActionId: actionId,
                    lastActionType: BallActionType.Kick,
                    lastKickDirection: localInitiator.KickDirection,
                    lastKickInitiatorNetId: localInitiator.NetId
                ));

                await UniTask.WaitUntil(
                    () => ball.StateHolder.GetState().LastActionId == actionId, cancellationToken: token
                );

                captureTokenSource?.Cancel();
                captureTokenSource = null;
                lastKickTime = Time.time;
            }
        }
    }
}
