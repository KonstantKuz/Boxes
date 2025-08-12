using Gameplay.Interactable.BallInteraction.Components;
using Infrastructure.Bootstrap;
using UnityEngine;

namespace Gameplay.Interactable.BallInteraction.Abstract
{
    public interface IBallInteractionMediator : IPostBuildInjectable
    {
        BallInteractionConfig Config { get; }
        void RegisterBall(Ball ball);
        void RegisterLocalInitiator(IBallInteractionInitiator initiator);
        bool IsBallOutOfBounds(out Plane outOfBoundsSide);
        bool IsPredictionVisible(out Vector3 direction);
    }
}
