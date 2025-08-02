using Infrastructure.WindowService.Abstract;
using UnityEngine;

namespace UI.Dialog
{
    [CreateAssetMenu(fileName = "DialogWindowType", menuName = "WindowType/DialogWindowType", order = 0)]
    public class DialogWindowType : WindowType
    {
        public override string Id => nameof(DialogWindow);
    }
}
