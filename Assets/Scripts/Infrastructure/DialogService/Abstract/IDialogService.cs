using System;
using Infrastructure.Bootstrap;

namespace Infrastructure.DialogService.Abstract
{
    public interface IDialogService : IPostBuildInjectable
    {
        void StartDialog(uint initiatorId, Guid dialogId);

        bool TryGetDialog(Guid dialogId, out DialogSequence dialogSequence);
    }
}
