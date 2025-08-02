using UnityEngine.InputSystem;

namespace Infrastructure.InputService.Abstract
{
    public interface IInputService
    {
        InputAction LookAction { get; }
        InputAction MoveAction { get; }
        InputAction JumpAction { get; }
        InputAction InteractAction { get; }
        InputAction NextAction { get; }

        void SwitchToDefaultContext();
        void SwitchToDialogContext();
    }
}
