using UnityEngine;

namespace Gameplay.Interactable.BallInteraction.Abstract
{
    public interface IBallInteractionInitiator
    {
        uint NetId { get; }
        Transform BallSocket { get; }
        Vector3 Position { get; }
        Vector3 KickDirection { get; }
        Rigidbody Rigidbody { get; }
        bool IsAimPressed { get; }
    }
}
