using System;

namespace Infrastructure.DialogService
{
    public class DialogState
    {
        public Guid DialogId { get; private set; }
        public uint CurrentIndex { get; private set; }

        public DialogState(Guid dialogId, uint currentIndex)
        {
            DialogId = dialogId;
            CurrentIndex = currentIndex;
        }
    }
}
