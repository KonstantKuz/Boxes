using System;

namespace Infrastructure.DialogService
{
    public interface IDialogService
    {
        DialogState CurrentState { get; }

        void CmdStartDialog(Guid dialogId);
        void CmdStopDialog();
        void CmdSetPlayerReady(int playerId);
    }
}
