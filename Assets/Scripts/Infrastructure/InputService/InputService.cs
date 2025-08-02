using System;
using Infrastructure.Bootstrap;
using Infrastructure.InputService.Abstract;
using UnityEngine.InputSystem;

namespace Infrastructure.InputService
{
    public class InputService : IInputService, IInitializable, IDisposable
    {
        private GameInput input;

        public InputAction LookAction => input.DefaultContext.Look;
        public InputAction MoveAction => input.DefaultContext.Move;
        public InputAction JumpAction => input.DefaultContext.Jump;
        public InputAction InteractAction => input.DefaultContext.Interact;
        public InputAction NextAction => input.DialogContext.Next;

        void IInitializable.Initialize()
        {
            input = new GameInput();
            input.Enable();
        }

        void IInputService.SwitchToDefaultContext()
        {
            input.DialogContext.Disable();
            input.DefaultContext.Enable();
        }

        void IInputService.SwitchToDialogContext()
        {
            input.DefaultContext.Disable();
            input.DialogContext.Enable();
        }

        public void Dispose()
        {
            input?.Dispose();
        }
    }
}
