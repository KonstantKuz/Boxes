using System;
using Gameplay.Interactable.BallInteraction;
using Gameplay.Interactable.BallInteraction.Abstract;
using Gameplay.Interactable.BallInteraction.Command;
using Gameplay.Interactable.BallInteraction.Components;
using Infrastructure.Bootstrap;
using Infrastructure.CameraService;
using Infrastructure.InputService.Abstract;
using Infrastructure.Network.Abstract;
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

        private Ball ball;
        private IBallInteractionInitiator initiator;

        [Inject]
        private void Construct(IInputService inputService, INetworkService networkService, ICameraService cameraService)
        {
            this.inputService = inputService;
            this.networkService = networkService;
            this.cameraService = cameraService;
        }

        void IInitializable.Initialize()
        {
            inputService.DefaultContextActions.Interact.performed += TryCaptureBall;
            inputService.DefaultContextActions.Action.performed += TryKickBall;
        }

        void IBallInteractionMediator.RegisterBall(Ball ball)
        {
            this.ball = ball;
        }

        void IBallInteractionMediator.RegisterLocalInitiator(IBallInteractionInitiator initiator)
        {
            this.initiator = initiator;
        }

        bool IBallInteractionMediator.IsBallOutOfBounds(out Plane outOfBoundsSide)
        {
            return !cameraService.IsVisible(ball.Bounds, out outOfBoundsSide);
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

            float distance = (initiator.Position - ball.transform.position).magnitude;

            if (distance <= ballInteractionConfig.KickMinDistance)
            {
                KickCommand command = new KickCommand(
                    initiator.NetId, initiator.KickDirection, ballInteractionConfig.MinSpeed
                );
                networkService.SendCommand(command);
            }
        }
    }
}
