namespace Infrastructure.InputService.Abstract
{
    public interface IInputService
    {
        GameInput.DefaultContextActions  DefaultContextActions { get; }
        GameInput.DialogContextActions  DialogContextActions { get; }

        void SwitchToDefaultContext();
        void SwitchToDialogContext();
    }
}
