using Gameplay.Interactable.BallInteraction.State;
using UnityEngine;

namespace Gameplay.Interactable.BallInteraction.Abstract
{
    public interface IBallReactionInitiator
    {
        bool TryExecuteReaction(Collision collisionInfo, BallSharedState sharedState);
    }
}
