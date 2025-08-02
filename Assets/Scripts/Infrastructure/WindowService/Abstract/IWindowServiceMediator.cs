using Infrastructure.Abstract;

namespace Infrastructure.WindowService.Abstract
{
    public interface IWindowServiceMediator : IServiceMediator
    {
        void AttachToCanvasRoot(IWindow window);
    }
}
