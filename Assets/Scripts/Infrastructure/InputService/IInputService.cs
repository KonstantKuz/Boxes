using R3;
using UnityEngine;

namespace Infrastructure.InputService
{
    public interface IInputService
    {
        ReactiveCommand<Vector2> LookInput { get; }
        ReactiveCommand<Vector2> MoveInput { get; }
        ReactiveCommand<Unit> InteractionInput { get; }
    }
}
