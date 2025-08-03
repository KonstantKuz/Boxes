using System;
using Infrastructure.Bootstrap;
using Infrastructure.DialogService;
using Infrastructure.DialogService.Abstract;
using Infrastructure.Network;
using Infrastructure.WindowService.Abstract;
using Reflex.Attributes;
using TMPro;
using UnityEngine;

namespace UI.Dialog
{
    public class DialogWindow : MonoBehaviour, IWindow, IInitializable
    {
        [SerializeField]
        private TextMeshProUGUI title;

        [SerializeField]
        private TextMeshProUGUI text;

        private INetworkService networkService;
        private IWindowService windowService;
        private IDialogService dialogService;
        private IDisposable dialogSubscription;

        public string Id => nameof(DialogWindow);

        [Inject]
        private void Construct(INetworkService networkService, IWindowService windowService, IDialogService dialogService)
        {
            this.networkService = networkService;
            this.windowService = windowService;
            this.dialogService = dialogService;
        }

        void IInitializable.Initialize()
        {
            windowService.RegisterWindow(Id, this);
        }

        void IWindow.Show(IWindowContext context)
        {
            gameObject.SetActive(true);

            dialogSubscription = networkService.ObserveState<DialogState>(OnDialogStateChanged);
        }

        void IWindow.Hide()
        {
            gameObject.SetActive(false);

            dialogSubscription?.Dispose();
            dialogSubscription = null;
        }

        private void OnDialogStateChanged(DialogState state)
        {
            if (state == null)
            {
                title.text = string.Empty;
                text.text = string.Empty;
                return;
            }

            if (
                !dialogService.TryGetDialog(state.DialogId, out DialogSequence dialogSequence) ||
                !dialogSequence.TryGetReplica(state.ReplicaIndex, out DialogReplica replica)
            )
            {
                return;
            }

            title.text = replica.Title.GetLocalizedString();
            text.text = replica.Message.GetLocalizedString();
        }
    }
}
