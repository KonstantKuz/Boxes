using Infrastructure.Bootstrap;

namespace Infrastructure.InputService.Abstract
{
    public interface IInputService : IPostBuildInjectable
    {
        GameInputActions GetInput(uint playerId);
        void SwitchToDefaultContext();
        void SwitchToDialogContext();
        void DisableAllInputs();
        void EnableAllInputs();
    }
}
