namespace Infrastructure.WindowService
{
    public interface IWindowService
    {
        IWindow ActiveWindow { get; }
        void RegisterWindow(string id, IWindow window);
        void ShowWindow(string id, IWindowContext context = null);
        void HideActiveWindow();
    }
}
