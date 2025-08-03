using Infrastructure.Bootstrap;

namespace Infrastructure.WindowService.Abstract
{
    public interface IWindowService : IPostBuildInjectable
    {
        IWindow ActiveWindow { get; }
        void RegisterWindow(string id, IWindow window);
        void ShowWindow(string id, IWindowContext context = null);
        void HideWindow(string id);
        void HideActiveWindow();
    }
}
