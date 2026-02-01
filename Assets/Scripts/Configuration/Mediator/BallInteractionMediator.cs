using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using Gameplay.Interactable.Abstract;
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
    public partial class BallInteractionMediator : InteractionMediatorBase<IBallInteractionInitiator>, IBallInteractionMediator, IInitializable, IUpdatable
    {
        [SerializeField]
        private BallInteractionConfig ballInteractionConfig;

        private IInputService inputService;
        private ICameraService cameraService;
        private INetworkService networkService;
        private ReactiveProperty<BallSharedState> stateReactive;

        private Ball ball;
        private Dictionary<uint, CancellationTokenSource> kickTokenSources;
        private Dictionary<uint, CancellationTokenSource> captureTokenSources;
        private Dictionary<uint, CancellationTokenSource> holdTokenSources;
        private Dictionary<uint, float> lastKickTimes;

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
            kickTokenSources = new Dictionary<uint, CancellationTokenSource>();
            captureTokenSources = new Dictionary<uint, CancellationTokenSource>();
            holdTokenSources = new Dictionary<uint, CancellationTokenSource>();
            lastKickTimes = new Dictionary<uint, float>();
        }

        void IInitializable.Initialize()
        {
        }

        void IBallInteractionMediator.RegisterBall(Ball ball)
        {
            this.ball = ball;
            this.ball.StateHolder.Subscribe(value => stateReactive.Value = value);
        }

        void IBallInteractionMediator.RegisterInitiator(IBallInteractionInitiator initiator, bool isLocalPlayer)
        {
            RegisterInitiatorInternal(initiator, initiator.NetId, isLocalPlayer);
        }

        bool IBallInteractionMediator.IsBallOutOfBounds(out Plane outOfBoundsSide)
        {
            outOfBoundsSide = default;
            return ball != null && !cameraService.IsVisible(ball.Bounds, out outOfBoundsSide);
        }

        void IUpdatable.Update()
        {
            foreach (var initiator in localInitiators)
            {
                if (initiator.IsAimPressed)
                {
                    ((IBallInteractionMediator)this).TryCaptureBall(initiator);
                }
            }
        }

        bool IBallInteractionMediator.IsPredictionVisible(IBallInteractionInitiator initiator, out Vector3 direction)
        {
            direction = Vector3.zero;

            if (initiator == null)
            {
                return false;
            }

            direction = initiator.KickDirection;

            float distance = Vector3.Distance(initiator.Position, ball.transform.position);
            bool isInRange = distance <= ballInteractionConfig.InteractionDistance;

            return isInRange && initiator.IsAimPressed;
        }

        bool IBallInteractionMediator.IsLocalInitiator(uint netId)
        {
            return IsLocalInitiator(netId);
        }

        void IBallInteractionMediator.TryHoldOrReleaseBall(IBallInteractionInitiator initiator)
        {
            if (ball == null || initiator == null)
            {
                return;
            }

            if (holdTokenSources.ContainsKey(initiator.NetId))
            {
                return;
            }

            BallSharedState currentState = ball.StateHolder.GetState();
            if (currentState.HasHolder && currentState.HolderNetId == initiator.NetId)
            {
                ball.StateHolder.WriteState(new BallSharedState(
                    kicksCount: currentState.KicksCount,
                    ownerNetId: initiator.NetId,
                    holderNetId: 0,
                    lastActionId: currentState.LastActionId + 1,
                    lastActionType: BallActionType.Release,
                    lastKickDirection: currentState.LastKickDirection,
                    lastKickInitiatorNetId: currentState.LastKickInitiatorNetId
                ));
                return;
            }

            holdTokenSources[initiator.NetId] = new CancellationTokenSource();

            TryHoldBallAsync(initiator, holdTokenSources[initiator.NetId].Token).Forget();

            async UniTask TryHoldBallAsync(IBallInteractionInitiator initiator, CancellationToken token)
            {
                float time = 0;

                while (!token.IsCancellationRequested && time < ballInteractionConfig.HoldWindowTime)
                {
                    time += Time.fixedDeltaTime;

                    float distance = (initiator.Position - ball.transform.position).magnitude;

                    if (distance <= ballInteractionConfig.InteractionDistance)
                    {
                        holdTokenSources.Remove(initiator.NetId);

                        if (captureTokenSources.TryGetValue(initiator.NetId, out var captureToken))
                        {
                            captureToken?.Cancel();
                            captureTokenSources.Remove(initiator.NetId);
                        }

                        if (kickTokenSources.TryGetValue(initiator.NetId, out var kickToken))
                        {
                            kickToken?.Cancel();
                            kickTokenSources.Remove(initiator.NetId);
                        }

                        BallSharedState current = ball.StateHolder.GetState();
                        byte kicksCount = ballInteractionConfig.ResetConditions.HasFlag(ResetCondition.Hold)
                            ? (byte)0
                            : current.KicksCount;

                        networkService.AssignAuthority(ball.netId, initiator.NetId);

                        ball.StateHolder.WriteState(new BallSharedState(
                            kicksCount: kicksCount,
                            ownerNetId: initiator.NetId,
                            holderNetId: initiator.NetId,
                            lastActionId: current.LastActionId + 1,
                            lastActionType: BallActionType.Hold,
                            lastKickDirection: Vector3.zero,
                            lastKickInitiatorNetId: current.LastKickInitiatorNetId
                        ));
                        break;
                    }

                    await UniTask.WaitForFixedUpdate();
                }

                holdTokenSources.Remove(initiator.NetId);
            }
        }

        void IBallInteractionMediator.TryKickBall(IBallInteractionInitiator initiator)
        {
            if (ball == null || initiator == null)
            {
                return;
            }

            if (kickTokenSources.ContainsKey(initiator.NetId))
            {
                return;
            }

            kickTokenSources[initiator.NetId] = new CancellationTokenSource();

            TryKickBallAsync(initiator, kickTokenSources[initiator.NetId].Token).Forget();

            async UniTask TryKickBallAsync(IBallInteractionInitiator initiator, CancellationToken token)
            {
                float time = 0;

                while (!token.IsCancellationRequested && time < ballInteractionConfig.KickWindowTime)
                {
                    time += Time.fixedDeltaTime;

                    float distance = (initiator.Position - ball.transform.position).magnitude;

                    if (distance <= ballInteractionConfig.InteractionDistance)
                    {
                        kickTokenSources.Remove(initiator.NetId);

                        if (captureTokenSources.TryGetValue(initiator.NetId, out var captureToken))
                        {
                            captureToken?.Cancel();
                            captureTokenSources.Remove(initiator.NetId);
                        }

                        lastKickTimes[initiator.NetId] = Time.time;

                        BallSharedState current = ball.StateHolder.GetState();

                        byte kicksCount = (byte)(current.KicksCount + 1);
                        kicksCount = (byte)Math.Clamp(kicksCount, 0, ballInteractionConfig.StatusKicksCount);

                        if (ballInteractionConfig.ResetConditions.HasFlag(ResetCondition.SamePlayerKick) &&
                            initiator.NetId == current.LastKickInitiatorNetId)
                        {
                            kicksCount = 1;
                        }

                        networkService.AssignAuthority(ball.netId, initiator.NetId);

                        ball.StateHolder.WriteState(new BallSharedState(
                            kicksCount: kicksCount,
                            ownerNetId: initiator.NetId,
                            holderNetId: 0,
                            lastActionId: current.LastActionId + 1,
                            lastActionType: BallActionType.Kick,
                            lastKickDirection: initiator.KickDirection,
                            lastKickInitiatorNetId: initiator.NetId
                        ));
                        break;
                    }

                    await UniTask.WaitForFixedUpdate();
                }

                kickTokenSources.Remove(initiator.NetId);
            }
        }

        void IBallInteractionMediator.TryCaptureBall(IBallInteractionInitiator initiator)
        {
            if (ballInteractionConfig.AutoCaptureTime <= 0)
            {
                return;
            }

            if (!lastKickTimes.TryGetValue(initiator.NetId, out float lastKickTime))
            {
                lastKickTime = 0;
            }

            float lastKickDeltaTime = Time.time - lastKickTime;
            bool isRecharged = lastKickDeltaTime > ballInteractionConfig.AutoCaptureRechargeTime;

            if (ball == null || initiator == null || captureTokenSources.ContainsKey(initiator.NetId)
                || kickTokenSources.ContainsKey(initiator.NetId) || !isRecharged || ball.StateHolder.GetState().HasHolder)
            {
                return;
            }

            float distance = (initiator.Position - ball.transform.position).magnitude;

            if (distance <= ballInteractionConfig.InteractionDistance)
            {
                captureTokenSources[initiator.NetId] = new CancellationTokenSource();

                TryCaptureBallAsync(initiator, captureTokenSources[initiator.NetId].Token).Forget();
            }

            async UniTask TryCaptureBallAsync(IBallInteractionInitiator initiator, CancellationToken token)
            {
                if (token.IsCancellationRequested)
                {
                    return;
                }

                networkService.AssignAuthority(ball.netId, initiator.NetId);

                BallSharedState current = ball.StateHolder.GetState();
                ball.StateHolder.WriteState(new BallSharedState(
                    kicksCount: current.KicksCount,
                    ownerNetId: initiator.NetId,
                    holderNetId: 0,
                    lastActionId: current.LastActionId + 1,
                    lastActionType: BallActionType.Capture,
                    lastKickDirection: initiator.BallSocket.InverseTransformPoint(ball.transform.position),
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
                    initiator.NetId == current.LastKickInitiatorNetId)
                {
                    kicksCount = 1;
                }

                uint actionId = ball.StateHolder.GetState().LastActionId + 1;

                ball.StateHolder.WriteState(new BallSharedState(
                    kicksCount: kicksCount,
                    ownerNetId: initiator.NetId,
                    holderNetId: 0,
                    lastActionId: actionId,
                    lastActionType: BallActionType.Kick,
                    lastKickDirection: initiator.KickDirection,
                    lastKickInitiatorNetId: initiator.NetId
                ));

                await UniTask.WaitUntil(
                    () => ball.StateHolder.GetState().LastActionId == actionId, cancellationToken: token
                );

                if (captureTokenSources.TryGetValue(initiator.NetId, out var captureToken))
                {
                    captureToken?.Cancel();
                    captureTokenSources.Remove(initiator.NetId);
                }

                lastKickTimes[initiator.NetId] = Time.time;
            }
        }
    }
}
