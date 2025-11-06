using System.Collections.Generic;
using Gameplay.Interactable.BallInteraction.Components;
using Gameplay.Interactable.BallInteraction.State;
using Infrastructure.Bootstrap;
using R3;
using UnityEngine;

namespace Gameplay.Interactable.BallInteraction.Abstract
{
    public interface IBallInteractionMediator : IPostBuildInjectable
    {
        Ball Ball { get; }
        BallInteractionConfig Config { get; }
        ReadOnlyReactiveProperty<BallSharedState>  BallState { get; }
        IReadOnlyDictionary<uint, IBallInteractionInitiator> Initiators { get; }
        IBallInteractionInitiator LocalInitiator { get; }
        void RegisterBall(Ball ball);
        void RegisterInitiator(IBallInteractionInitiator initiator, bool isLocalPlayer);
        bool IsBallOutOfBounds(out Plane outOfBoundsSide);
        bool IsPredictionVisible(out Vector3 direction);
    }
}
