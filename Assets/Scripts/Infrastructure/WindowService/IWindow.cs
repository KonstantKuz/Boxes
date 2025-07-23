namespace Infrastructure.WindowService
{
    public interface IWindow
    {
        string Id { get; }
        void Show(IWindowContext context = null);
        void Hide();
    }
}
