using Infrastructure.Bootstrap;

namespace Infrastructure.WindowService.Abstract
{
    public interface IWindowServiceMediator : IPostBuildInjectable
    {
        void AttachToCanvasRoot(IWindow window);
    }
}
