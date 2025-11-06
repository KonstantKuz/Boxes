using UnityEngine;

namespace Gameplay.Interactable.PipeInteraction.Abstract
{
    public interface IPipeInteractionInitiator
    {
        uint NetId { get; }
        Rigidbody Rigidbody { get; }
    }
}
