using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Gameplay.Interactable.BallInteraction;
using Gameplay.Interactable.BallInteraction.Abstract;
using Gameplay.Interactable.BallInteraction.Command;
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
    public partial class BallInteractionMediator : IBallInteractionMediator, IInitializable
    {
        [SerializeField]
        private BallInteractionConfig ballInteractionConfig;

        private IInputService inputService;
        private INetworkService networkService;
        private ICameraService cameraService;
        private ReactiveProperty<BallSharedState> stateReactive;
        private Dictionary<uint, IBallInteractionInitiator> initiators;

        private Ball ball;
        private IBallInteractionInitiator initiator;
        private CancellationTokenSource kickTokenSource;
        private CancellationTokenSource captureTokenSource;
        private float lastKickTime;

        BallInteractionConfig IBallInteractionMediator.Config => ballInteractionConfig;
        ReadOnlyReactiveProperty<BallSharedState> IBallInteractionMediator.BallState => stateReactive;

        IReadOnlyDictionary<uint, IBallInteractionInitiator> IBallInteractionMediator.Initiators => initiators;

        [Inject]
        private void Construct(IInputService inputService, INetworkService networkService, ICameraService cameraService)
        {
            this.inputService = inputService;
            this.networkService = networkService;
            this.cameraService = cameraService;

            stateReactive = new ReactiveProperty<BallSharedState>(BallSharedState.Default);
            initiators = new Dictionary<uint, IBallInteractionInitiator>();
        }

        void IInitializable.Initialize()
        {
            inputService.DefaultContextActions.Take.performed += TryHoldBall;
            inputService.DefaultContextActions.Action.performed += TryKickBall;
            inputService.DefaultContextActions.Aim.performed += TryCaptureBall;
        }

        void IBallInteractionMediator.RegisterBall(Ball ball)
        {
            this.ball = ball;
            this.ball.StateHolder.Subscribe(value => stateReactive.Value = value);
        }

        void IBallInteractionMediator.RegisterInitiator(uint netId, IBallInteractionInitiator initiator, bool isLocalPlayer)
        {
            if (isLocalPlayer)
            {
                this.initiator = initiator;
            }

            initiators.Add(netId, initiator);
        }

        bool IBallInteractionMediator.IsBallOutOfBounds(out Plane outOfBoundsSide)
        {
            return !cameraService.IsVisible(ball.Bounds, out outOfBoundsSide);
        }

        bool IBallInteractionMediator.IsPredictionVisible(out Vector3 direction)
        {
            direction = Vector3.zero;

            if (initiator == null)
            {
                return false;
            }

            direction = initiator.KickDirection;

            float distance = Vector3.Distance(initiator.Position, ball.transform.position);
            bool isInRange = distance < ballInteractionConfig.InteractionDistance + ball.transform.localScale.x / 2;

            return isInRange && inputService.DefaultContextActions.Aim.IsPressed();
        }

        private void TryHoldBall(InputAction.CallbackContext context)
        {
            if (ball == null)
            {
                return;
            }

            float distance = (initiator.Position - ball.transform.position).magnitude;

            if (distance <= ballInteractionConfig.InteractionDistance)
            {
                networkService.SendCommand(new HoldCommand(initiator.NetId));
                captureTokenSource?.Cancel();
                kickTokenSource?.Cancel();
            }
        }

        private void TryKickBall(InputAction.CallbackContext context)
        {
            if (ball == null || kickTokenSource != null)
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

                    float distance = (initiator.Position - ball.transform.position).magnitude;

                    if (distance <= ballInteractionConfig.InteractionDistance)
                    {
                        kickTokenSource = null;
                        captureTokenSource?.Cancel();
                        captureTokenSource = null;
                        lastKickTime = Time.time;
                        KickCommand command = new KickCommand(initiator.NetId, initiator.KickDirection);
                        networkService.SendCommand(command);
                        break;
                    }

                    await UniTask.WaitForFixedUpdate();
                }

                kickTokenSource = null;
            }
        }

        private void TryCaptureBall(InputAction.CallbackContext context)
        {
            if (ballInteractionConfig.AutoCaptureTime <= 0)
            {
                return;
            }

            float lastKickDeltaTime = Time.time - lastKickTime;
            bool isRecharged = lastKickDeltaTime > ballInteractionConfig.AutoCaptureRechargeTime;

            if (ball == null || captureTokenSource != null || kickTokenSource != null || !isRecharged ||
                ball.StateHolder.GetState().HasHolder)
            {
                return;
            }

            float distance = (initiator.Position - ball.transform.position).magnitude;

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

                Vector3 relativePosition = initiator.BallSocket.InverseTransformPoint(ball.transform.position);
                networkService.SendCommand(new CaptureCommand(initiator.NetId, relativePosition));

                float elapsedTime = 0;
                while (elapsedTime < ballInteractionConfig.AutoCaptureTime)
                {
                    elapsedTime += Time.fixedDeltaTime;
                    await UniTask.WaitForFixedUpdate();
                }

                captureTokenSource = null;

                if (token.IsCancellationRequested)
                {
                    return;
                }

                KickCommand command = new KickCommand(initiator.NetId, initiator.KickDirection);
                networkService.SendCommand(command);

                lastKickTime = Time.time;

                kickTokenSource?.Cancel();
                kickTokenSource = null;
            }
        }
    }
}
