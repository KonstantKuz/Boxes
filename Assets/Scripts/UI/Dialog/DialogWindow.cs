using System;
using Infrastructure.Bootstrap;
using Infrastructure.DialogService;
using Infrastructure.DialogService.Abstract;
using Infrastructure.DialogService.State;
using Infrastructure.Network.Abstract;
using Infrastructure.Network.State;
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

        [SerializeField]
        private TextMeshProUGUI readyPlayersCounter;

        private IWindowService windowService;
        private IDialogService dialogService;
        private INetworkStateHolder<DialogState> dialogStateHolder;
        private INetworkStateHolder<ConnectionState> connectionStateHolder;

        private IDisposable dialogSubscription;

        public string Id => nameof(DialogWindow);

        [Inject]
        private void Construct(
            IWindowService windowService,
            IDialogService dialogService,
            INetworkStateHolder<DialogState> dialogStateHolder,
            INetworkStateHolder<ConnectionState>  connectionStateHolder
        )
        {
            this.windowService = windowService;
            this.dialogService = dialogService;
            this.dialogStateHolder = dialogStateHolder;
            this.connectionStateHolder = connectionStateHolder;
        }

        void IInitializable.Initialize()
        {
            windowService.RegisterWindow(Id, this);
        }

        void IWindow.Show(IWindowContext context)
        {
            gameObject.SetActive(true);

            dialogSubscription = dialogStateHolder.Subscribe(OnDialogStateChanged);
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

            int playersCount = connectionStateHolder.GetState().Players.Count;

            readyPlayersCounter.text = playersCount > 1
                ? $"{state.ReadyPlayers?.Count ?? 0}/{playersCount}"
                : string.Empty;
        }
    }
}
