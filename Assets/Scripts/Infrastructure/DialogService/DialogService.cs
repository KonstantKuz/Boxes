using System;
using Infrastructure.Bootstrap;
using Infrastructure.DialogService.Abstract;
using Reflex.Attributes;
// ReSharper disable ConvertToAutoProperty

namespace Infrastructure.DialogService
{
    [Serializable]
    public class DialogService : IDialogService, IPostBuildInjectable
    {
        private IDialogServiceMediator mediator;

        [Inject]
        private void Construct(IDialogServiceMediator mediator)
        {
            this.mediator = mediator;
        }

        void IDialogService.StartDialog(uint initiatorId, Guid dialogId)
        {
            mediator.StartDialog(initiatorId, dialogId);
        }

        bool IDialogService.TryGetDialog(Guid dialogId, out DialogSequence dialogSequence)
        {
            return mediator.TryGetDialog(dialogId, out dialogSequence);
        }
    }
}
