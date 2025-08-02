using System;
using Infrastructure.Abstract;
using Infrastructure.DialogService.Abstract;
using Infrastructure.InputService.Abstract;
using Infrastructure.WindowService.Abstract;
using Mirror;
using R3;
using Reflex.Attributes;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Configuration
{
    [Serializable]
    public class DialogServiceMediator : NetworkBehaviour, IDialogServiceMediator
    {
        [SerializeField]
        private WindowType dialogWindowType;

        private IWindowService windowService;
        private IInputService inputService;
        private ReactiveCommand<Unit> onLocalPlayerReady;

        Type IServiceMediator.BindType => typeof(IDialogServiceMediator);
        ReactiveCommand<Unit> IDialogServiceMediator.OnLocalPlayerReady => onLocalPlayerReady;

        [Inject]
        private void Construct(IWindowService windowService, IInputService inputService)
        {
            this.windowService = windowService;
            this.inputService = inputService;

            onLocalPlayerReady = new ReactiveCommand<Unit>();
        }

        [Command(requiresAuthority = false)]
        void IDialogServiceMediator.CmdStartDialog()
        {
            RpcStartDialog();
        }

        [Command(requiresAuthority = false)]
        void IDialogServiceMediator.CmdStopDialog()
        {
            RpcStopDialog();
        }

        [ClientRpc]
        private void RpcStartDialog()
        {
            windowService.ShowWindow(dialogWindowType.Id);
            inputService.SwitchToDialogContext();
            inputService.NextAction.performed += NotifyLocalPlayerReady;
        }

        private void NotifyLocalPlayerReady(InputAction.CallbackContext context)
        {
            onLocalPlayerReady.Execute(Unit.Default);
        }

        [ClientRpc]
        private void RpcStopDialog()
        {
            windowService.HideWindow(dialogWindowType.Id);
            inputService.SwitchToDefaultContext();
            inputService.NextAction.performed -= NotifyLocalPlayerReady;
        }
    }
}
