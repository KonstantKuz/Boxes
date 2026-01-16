using CMF;
using Gameplay.Interactable.BoxesInteraction.Components;
using UnityEngine;

namespace Gameplay.Interactable.BoxesInteraction.Abstract
{
    public interface IBoxInteractionInitiator
    {
        uint NetId { get; }
        Transform Socket { get; }
        Box CurrentBox { get; }
        AdvancedWalkerController Controller { get; }
    }
}
