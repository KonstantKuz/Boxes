using System;
using Infrastructure.Abstract;

namespace Infrastructure.DialogService.Abstract
{
    public interface IDialogServiceMediator : IServiceMediator
    {
        void StartDialog(uint initiatorId, Guid dialogId);

        bool TryGetDialog(Guid dialogId, out DialogSequence dialogSequence);
    }
}
