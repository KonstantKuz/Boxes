using Infrastructure.Abstract;
using R3;

namespace Infrastructure.DialogService.Abstract
{
    public interface IDialogServiceMediator : IServiceMediator
    {
        ReactiveCommand<Unit> OnLocalPlayerReady { get; }

        void CmdStartDialog();
        void CmdStopDialog();
    }
}
