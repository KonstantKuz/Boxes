using Infrastructure.Bootstrap;

namespace Infrastructure.WindowService.Abstract
{
    public interface IWindow : IPostBuildInjectable
    {
        string Id { get; }
        void Show(IWindowContext context = null);
        void Hide();
    }
}
