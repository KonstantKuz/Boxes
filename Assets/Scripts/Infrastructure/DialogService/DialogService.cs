using System;
using System.Collections.Generic;
using System.Linq;
using Infrastructure.Bootstrap;
using Infrastructure.DialogService.Abstract;
using Mirror;
using R3;
using Reflex.Attributes;
using UnityEngine;
// ReSharper disable ConvertToAutoProperty

namespace Infrastructure.DialogService
{
    public class DialogService : NetworkBehaviour, IDialogService, IPostBuildInjectable
    {
        [SerializeField]
        private List<DialogSequence> dialogs;

        private IDialogServiceMediator mediator;
        private Dictionary<Guid, DialogSequence> dialogsMap;
        private ReactiveProperty<DialogState> currentDialogStateReactive;
        private IDisposable localPlayerReadySubscription;

        [SyncVar(hook = nameof(OnCurrentDialogStateChanged))]
        private DialogState currentDialogStateSync;

        ReadOnlyReactiveProperty<DialogState> IDialogService.CurrentSync => currentDialogStateReactive;

        [Inject]
        private void Construct(IDialogServiceMediator mediator)
        {
            this.mediator = mediator;
            dialogsMap = dialogs.ToDictionary(item => item.Id, item => item);
            currentDialogStateReactive = new ReactiveProperty<DialogState>();
        }

        public override void OnStartServer()
        {
            currentDialogStateSync = DialogState.Default;
        }

        private void OnCurrentDialogStateChanged(DialogState oldState, DialogState newState)
        {
            currentDialogStateReactive.Value = newState;
        }

        [Command(requiresAuthority = false)]
        private void CmdSetPlayerReady(uint playerId)
        {
            if (
                currentDialogStateSync == null ||
                currentDialogStateSync == DialogState.Default ||
                !dialogsMap.TryGetValue(currentDialogStateSync.DialogId, out DialogSequence dialogSequence)
            )
            {
                Debug.LogError($"Dialog with ID {currentDialogStateSync?.DialogId} not found.");
                return;
            }

            HashSet<uint> readyPlayers = currentDialogStateSync.ReadyPlayers ?? new HashSet<uint>();

            if (!readyPlayers.Add(playerId))
            {
                return;
            }

            byte replicaIndex = currentDialogStateSync.ReplicaIndex;

            if (NetworkServer.connections.Values.All(item => readyPlayers.Contains(item.identity.netId)))
            {
                replicaIndex++;
                readyPlayers.Clear();
                if (replicaIndex >= dialogSequence.Replicas.Length)
                {
                    ((IDialogService)this).CmdStopDialog();
                    return;
                }
            }

            currentDialogStateSync = new DialogState(
                currentDialogStateSync.DialogId,
                replicaIndex,
                readyPlayers
            );
        }

        [ClientRpc]
        private void RpcOnStartDialog()
        {
            localPlayerReadySubscription =
                mediator.OnLocalPlayerReady.Subscribe(_ => CmdSetPlayerReady(NetworkClient.localPlayer.netId));
        }

        [ClientRpc]
        private void RpcOnStopDialog()
        {
            localPlayerReadySubscription?.Dispose();
            localPlayerReadySubscription = null;
        }

        [Command(requiresAuthority = false)]
        void IDialogService.CmdStartDialog(Guid dialogId)
        {
            if (!dialogsMap.TryGetValue(dialogId, out DialogSequence _))
            {
                Debug.LogError($"Dialog with ID {dialogId} not found.");
                return;
            }

            currentDialogStateSync = new DialogState(dialogId, 0, null);
            mediator.CmdStartDialog();
            RpcOnStartDialog();
        }

        [Command(requiresAuthority = false)]
        void IDialogService.CmdStopDialog()
        {
            currentDialogStateSync = DialogState.Default;
            mediator.CmdStopDialog();
            RpcOnStopDialog();
        }

        bool IDialogService.TryGetDialog(Guid dialogId, out DialogSequence dialogSequence)
        {
            return dialogsMap.TryGetValue(dialogId, out dialogSequence);
        }
    }
}
