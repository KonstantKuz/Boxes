using Infrastructure.DialogService;
using Infrastructure.WindowService.Abstract;

namespace UI.Dialog
{
    public class DialogWindowContext : IWindowContext
    {
        public DialogSequence DialogSequence { get; }

        public DialogWindowContext(DialogSequence dialogSequence)
        {
            DialogSequence = dialogSequence;
        }
    }
}
