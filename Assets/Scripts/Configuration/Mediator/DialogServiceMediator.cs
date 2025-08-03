using System;
using System.Collections.Generic;
using System.Linq;
using Infrastructure.Bootstrap;
using Infrastructure.DialogService;
using Infrastructure.DialogService.Abstract;
using Infrastructure.DialogService.Command;
using Infrastructure.InputService.Abstract;
using Infrastructure.Network;
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
        private Dictionary<Guid, DialogSequence> dialogsMap;

        [Inject]
        private void Construct(INetworkService networkService, IWindowService windowService, IInputService inputService)
        {
            this.networkService = networkService;
            this.windowService = windowService;
            this.inputService = inputService;
        }

        void IInitializable.Initialize()
        {
            dialogsMap = dialogs.ToDictionary(item => item.Id, item => item);

            networkService.ObserveReaction<StartDialogCommand>(StartLocalDialog);
            networkService.ObserveReaction<StopDialogCommand>(StopLocalDialog);

            networkService.ObserveCommand<StartDialogCommand>(CreateDialogState);
            networkService.ObserveCommand<ReadyDialogCommand>(UpdateDialogState);
            networkService.ObserveCommand<StopDialogCommand>(CleanDialogState);
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
            networkService.WriteState(state);
        }

        private void StartLocalDialog(StartDialogCommand command)
        {
            windowService.ShowWindow(dialogWindowType.Id);
            inputService.SwitchToDialogContext();
            inputService.NextAction.performed += OnLocalPlayerReady;
        }

        private void OnLocalPlayerReady(InputAction.CallbackContext context)
        {
            networkService.SendCommand(new ReadyDialogCommand {InitiatorId = NetworkClient.localPlayer.netId});
        }

        private void StopLocalDialog(StopDialogCommand command)
        {
            windowService.HideWindow(dialogWindowType.Id);
            inputService.SwitchToDefaultContext();
            inputService.NextAction.performed -= OnLocalPlayerReady;
        }

        private void UpdateDialogState(ReadyDialogCommand command)
        {
            DialogState dialogState = networkService.ReadState<DialogState>();

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
            networkService.WriteState(updatedState);
        }

        private void CleanDialogState(StopDialogCommand command)
        {
            networkService.WriteState(DialogState.Default);
        }
    }
}
