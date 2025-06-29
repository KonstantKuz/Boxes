using Reflex.Core;

namespace Infrastructure.InputService
{
    // ReSharper disable once UnusedType.Global
    public class InputServiceInstaller : IInstaller
    {
        public void InstallBindings(ContainerBuilder containerBuilder)
        {
            InputService inputService = new InputService();
            inputService.Initialize();
            containerBuilder.AddSingleton(inputService, typeof(IInputService));
        }
    }
}
