using System;
using R3;

namespace Infrastructure.DialogService.Abstract
{
    public interface IDialogService
    {
        ReadOnlyReactiveProperty<DialogState> CurrentSync { get; }

        void CmdStartDialog(Guid dialogId);
        void CmdStopDialog();

        bool TryGetDialog(Guid dialogId, out DialogSequence dialogSequence);
    }
}
