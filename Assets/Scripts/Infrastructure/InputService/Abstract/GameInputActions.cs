namespace Infrastructure.InputService.Abstract
{
    public class GameInputActions
    {
        public GameInput.DefaultContextActions DefaultContext { get; }
        public GameInput.DialogContextActions DialogContext { get; }

        public GameInputActions(GameInput gameInput)
        {
            DefaultContext = gameInput.DefaultContext;
            DialogContext = gameInput.DialogContext;
        }
    }
}
