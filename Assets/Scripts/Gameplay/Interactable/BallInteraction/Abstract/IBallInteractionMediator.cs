using Gameplay.Interactable.BallInteraction.Components;
using Gameplay.Interactable.BallInteraction.State;
using Infrastructure.Bootstrap;
using R3;
using UnityEngine;

namespace Gameplay.Interactable.BallInteraction.Abstract
{
    public interface IBallInteractionMediator : IPostBuildInjectable
    {
        BallInteractionConfig Config { get; }
        ReadOnlyReactiveProperty<BallState>  BallState { get; }
        void RegisterBall(Ball ball);
        void RegisterLocalInitiator(IBallInteractionInitiator initiator);
        bool IsBallOutOfBounds(out Plane outOfBoundsSide);
        bool IsPredictionVisible(out Vector3 direction);
    }
}
