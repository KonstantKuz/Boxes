using System;
using Infrastructure.Bootstrap;
using Infrastructure.InputService.Abstract;

namespace Infrastructure.InputService
{
    [Serializable]
    public class InputService : IInputService, IInitializable, IDisposable
    {
        private GameInput input;

        GameInput.DefaultContextActions IInputService.DefaultContextActions => input.DefaultContext;
        GameInput.DialogContextActions IInputService.DialogContextActions => input.DialogContext;

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
