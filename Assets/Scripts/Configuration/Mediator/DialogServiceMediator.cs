using System;
using System.Collections.Generic;
using System.Linq;
using Infrastructure.Bootstrap;
using Infrastructure.DialogService;
using Infrastructure.DialogService.Abstract;
using Infrastructure.DialogService.Command;
using Infrastructure.DialogService.State;
using Infrastructure.InputService.Abstract;
using Infrastructure.Network.Abstract;
using Infrastructure.WindowService.Abstract;
using Mirror;
using Reflex.Attributes;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Configuration.Mediator
{
    [Serializable]
    public class DialogServiceMediator : IDialogServiceMediator, IInitializable
    {
        [SerializeField]
        private List<DialogSequence> dialogs;

        [SerializeField]
        private WindowType dialogWindowType;

        private INetworkService networkService;
        private IWindowService windowService;
        private IInputService inputService;
        private INetworkStateHolder<DialogState > dialogStateHolder;

        private Dictionary<Guid, DialogSequence> dialogsMap;

        [Inject]
        private void Construct(
            INetworkService networkService,
            IWindowService windowService,
            IInputService inputService,
            INetworkStateHolder<DialogState> dialogStateHolder
        )
        {
            this.networkService = networkService;
            this.windowService = windowService;
            this.inputService = inputService;
            this.dialogStateHolder = dialogStateHolder;
        }

        void IInitializable.Initialize()
        {
            dialogsMap = dialogs.ToDictionary(item => item.Id, item => item);

            networkService.ObserveToReact<StartDialogCommand>(StartLocalDialog);
            networkService.ObserveToReact<StopDialogCommand>(StopLocalDialog);

            networkService.ObserveToExecute<StartDialogCommand>(CreateDialogState);
            networkService.ObserveToExecute<ReadyDialogCommand>(UpdateDialogState);
            networkService.ObserveToExecute<StopDialogCommand>(CleanDialogState);
        }

        void IDialogServiceMediator.StartDialog(uint initiatorId, Guid dialogId)
        {
            StartDialogCommand command = new StartDialogCommand {DialogId = dialogId, InitiatorId = initiatorId};
            networkService.SendCommand(command);
        }

        bool IDialogServiceMediator.TryGetDialog(Guid dialogId, out DialogSequence dialogSequence)
        {
            return dialogsMap.TryGetValue(dialogId, out dialogSequence);
        }

        private void CreateDialogState(StartDialogCommand command)
        {
            DialogState state = new DialogState(command.DialogId, 0, null);
            dialogStateHolder.WriteState(state);
        }

        private void StartLocalDialog(StartDialogCommand command)
        {
            windowService.ShowWindow(dialogWindowType.Id);
            inputService.SwitchToDialogContext();
            inputService.DialogContextActions.Next.performed += OnLocalPlayerReady;
        }

        private void OnLocalPlayerReady(InputAction.CallbackContext context)
        {
            networkService.SendCommand(new ReadyDialogCommand {InitiatorId = NetworkClient.localPlayer.netId});
        }

        private void StopLocalDialog(StopDialogCommand command)
        {
            windowService.HideWindow(dialogWindowType.Id);
            inputService.SwitchToDefaultContext();
            inputService.DialogContextActions.Next.performed -= OnLocalPlayerReady;
        }

        private void UpdateDialogState(ReadyDialogCommand command)
        {
            DialogState dialogState = dialogStateHolder.GetState();

            HashSet<uint> readyPlayers = dialogState.ReadyPlayers ?? new HashSet<uint>();

            if (!readyPlayers.Add(command.InitiatorId))
            {
                return;
            }

            byte replicaIndex = dialogState.ReplicaIndex;
            IEnumerable<uint> connectedPlayers =
                NetworkServer.connections.Values.Select(item => item.identity.netId);

            DialogSequence sequence = dialogsMap[dialogState.DialogId];

            if (connectedPlayers.All(netId => readyPlayers.Contains(netId)))
            {
                replicaIndex++;
                readyPlayers.Clear();
                if (replicaIndex >= sequence.Replicas.Length)
                {
                    networkService.SendCommand(new StopDialogCommand());
                    return;
                }
            }

            DialogState updatedState = new DialogState(dialogState.DialogId, replicaIndex, readyPlayers);
            dialogStateHolder.WriteState(updatedState);
        }

        private void CleanDialogState(StopDialogCommand command)
        {
            dialogStateHolder.WriteState(DialogState.Default);
        }
    }
}
