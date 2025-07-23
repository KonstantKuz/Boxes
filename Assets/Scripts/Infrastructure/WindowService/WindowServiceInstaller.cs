using Reflex.Core;

namespace Infrastructure.WindowService
{
    // ReSharper disable once UnusedType.Global
    public class WindowServiceInstaller : IInstaller
    {
        public void InstallBindings(ContainerBuilder containerBuilder)
        {
            containerBuilder.AddSingleton(typeof(WindowService), typeof(IWindowService));
        }
    }
}
