namespace Infrastructure.WindowService.Abstract
{
    public interface IWindowService
    {
        IWindow ActiveWindow { get; }
        void RegisterWindow(string id, IWindow window);
        void ShowWindow(string id, IWindowContext context = null);
        void HideWindow(string id);
        void HideActiveWindow();
    }
}
