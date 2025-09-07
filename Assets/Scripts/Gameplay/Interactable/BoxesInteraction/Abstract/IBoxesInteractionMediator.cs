using System.Collections.Generic;
using Infrastructure.Bootstrap;
using UnityEngine;

namespace Gameplay.Interactable.BoxesInteraction.Abstract
{
    public interface IBoxesInteractionMediator : IPostBuildInjectable
    {
        BoxesInteractionConfig Config { get; }
        IReadOnlyDictionary<uint, IBoxInteractionInitiator> Initiators { get; }
        void RegisterInitiator(IBoxInteractionInitiator initiator, bool isLocal);
        bool IsPredictionVisible(out Vector3 targetPosition);
        Vector3 CalculateLandingPoint(Vector3 startPosition, Vector3 initialVelocity);
    }
}
