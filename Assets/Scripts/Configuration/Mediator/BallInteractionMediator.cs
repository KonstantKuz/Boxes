using System;
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
        private float kickInputPassTime;

        [SerializeField]
        private BallInteractionConfig ballInteractionConfig;

        private IInputService inputService;
        private INetworkService networkService;
        private ICameraService cameraService;
        private ReactiveProperty<BallState> stateReactive;

        private Ball ball;
        private IBallInteractionInitiator initiator;
        private CancellationTokenSource kickTokenSource;

        BallInteractionConfig IBallInteractionMediator.Config => ballInteractionConfig;
        ReadOnlyReactiveProperty<BallState> IBallInteractionMediator.BallState => stateReactive;

        [Inject]
        private void Construct(IInputService inputService, INetworkService networkService, ICameraService cameraService)
        {
            this.inputService = inputService;
            this.networkService = networkService;
            this.cameraService = cameraService;

            stateReactive = new ReactiveProperty<BallState>(BallState.Default);
        }

        void IInitializable.Initialize()
        {
            inputService.DefaultContextActions.Interact.performed += TryCaptureBall;
            inputService.DefaultContextActions.Action.performed += TryKickBall;
        }

        void IBallInteractionMediator.RegisterBall(Ball ball)
        {
            this.ball = ball;
            this.ball.StateHolder.Subscribe(value => stateReactive.Value = value);
        }

        void IBallInteractionMediator.RegisterLocalInitiator(IBallInteractionInitiator initiator)
        {
            this.initiator = initiator;
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
            bool isInRange = distance < ballInteractionConfig.KickMinDistance + ball.transform.localScale.x / 2;

            return isInRange && inputService.DefaultContextActions.Aim.IsPressed();
        }

        private void TryCaptureBall(InputAction.CallbackContext context)
        {
            if (ball == null)
            {
                return;
            }

            float distance = (initiator.Position - ball.transform.position).magnitude;

            if (distance <= ballInteractionConfig.CaptureMinDistance)
            {
                networkService.SendCommand(new CaptureCommand(initiator.NetId));
            }
        }

        private void TryKickBall(InputAction.CallbackContext context)
        {
            if (ball == null)
            {
                return;
            }

            kickTokenSource?.Cancel();
            kickTokenSource = new CancellationTokenSource();

            TryKickBallAsync(kickTokenSource.Token).Forget();

            async UniTask TryKickBallAsync(CancellationToken token)
            {
                float time = 0;

                while (!token.IsCancellationRequested && time < kickInputPassTime)
                {
                    time += Time.fixedDeltaTime;

                    float distance = (initiator.Position - ball.transform.position).magnitude;

                    if (distance <= ballInteractionConfig.KickMinDistance)
                    {
                        KickCommand command = new KickCommand(
                            initiator.NetId, initiator.KickDirection, ballInteractionConfig.MinSpeed
                        );
                        networkService.SendCommand(command);
                        break;
                    }

                    await UniTask.WaitForFixedUpdate();
                }
            }
        }
    }
}
