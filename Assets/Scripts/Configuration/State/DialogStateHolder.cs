using Infrastructure.DialogService;
using Infrastructure.Network;

namespace Configuration.State
{
    public class DialogStateHolder : NetworkStateHolderBase
    {
        public override void OnStartServer()
        {
            base.OnStartServer();

            WriteState(DialogState.Default);
        }

        private void OnEnable()
        {
            NetworkService.RegisterStateHolder<DialogState>(this);
        }

        private void OnDisable()
        {
            NetworkService.UnregisterStateHolder<DialogState>(this);
        }
    }
}
