using UnityEngine;

namespace Gameplay.Interactable.PipeInteraction.Abstract
{
    public interface IPipeInteractionInitiator
    {
        uint NetId { get; }
        Vector3 Position { get; }
    }
}
