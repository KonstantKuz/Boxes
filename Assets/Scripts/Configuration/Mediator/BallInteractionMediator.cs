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
    public partial class BallInteractionMediator : IBallInteractionMediator, IInitializable, IUpdatable
    {
        [SerializeField]
        private BallInteractionConfig ballInteractionConfig;

        private IInputService inputService;
        private INetworkService networkService;
        private ICameraService cameraService;
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

        private void TryHoldBall(InputAction.CallbackContext context)
        {
            if (ball == null || localInitiator == null || holdTokenSource != null)
            {
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

                        networkService.SendCommand(new HoldCommand(localInitiator.NetId));
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
                        KickCommand command = new KickCommand(localInitiator.NetId, localInitiator.KickDirection);
                        networkService.SendCommand(command);
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

                Vector3 relativePosition = localInitiator.BallSocket.InverseTransformPoint(ball.transform.position);
                networkService.SendCommand(new CaptureCommand(localInitiator.NetId, relativePosition));

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

                KickCommand command = new KickCommand(localInitiator.NetId, localInitiator.KickDirection);
                networkService.SendCommand(command);

                lastKickTime = Time.time;

                kickTokenSource?.Cancel();
                kickTokenSource = null;
            }
        }
    }
}
