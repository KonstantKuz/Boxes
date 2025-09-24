using System;
using Infrastructure.Bootstrap;

namespace Infrastructure.DialogService.Abstract
{
    public interface IDialogService : IPostBuildInjectable
    {
        IDisposable StartDialog(uint initiatorId, Guid dialogId);

        bool TryGetDialog(Guid dialogId, out DialogSequence dialogSequence);
    }
}
