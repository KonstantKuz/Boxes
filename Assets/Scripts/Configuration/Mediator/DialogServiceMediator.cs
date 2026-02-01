using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Infrastructure;
using Infrastructure.Bootstrap;
using Infrastructure.DialogService;
using Infrastructure.DialogService.Abstract;
using Infrastructure.DialogService.Command;
using Infrastructure.DialogService.State;
using Infrastructure.InputService.Abstract;
using Infrastructure.Network.Abstract;
using Infrastructure.Network.State;
using Infrastructure.WindowService.Abstract;
using Mirror;
using R3;
using Reflex.Attributes;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Configuration.Mediator
{
    [Serializable]
    public class DialogServiceMediator : IDialogServiceMediator, IInitializable
    {
        [SerializeField]
        private WindowType dialogWindowType;

        private INetworkService networkService;
        private INetworkManager networkManager;
        private INetworkFactory networkFactory;
        private IWindowService windowService;
        private IInputService inputService;
        private INetworkStateHolder<DialogState> dialogStateHolder;

        private Dictionary<Guid, DialogSequence> dialogsMap;
        private Dictionary<uint, DialogLocalPlayer> localPlayers;

        [Inject]
        private void Construct(
            INetworkService networkService,
            INetworkManager networkManager,
            INetworkFactory networkFactory,
            IWindowService windowService,
            IInputService inputService,
            INetworkStateHolder<DialogState> dialogStateHolder
        )
        {
            this.networkService = networkService;
            this.networkManager = networkManager;
            this.networkFactory = networkFactory;
            this.windowService = windowService;
            this.inputService = inputService;
            this.dialogStateHolder = dialogStateHolder;

            localPlayers = new Dictionary<uint, DialogLocalPlayer>();
        }

        void IInitializable.Initialize()
        {
            LoadDialogsAsync().Forget();

            networkManager.ConnectionState.Subscribe(OnConnectionStateChanged);

            networkService.ObserveToReact<StartDialogCommand>(StartLocalDialog);
            networkService.ObserveToReact<StopDialogCommand>(StopLocalDialog);

            networkService.ObserveToExecute<StartDialogCommand>(CreateDialogState);
            networkService.ObserveToExecute<ReadyDialogCommand>(UpdateDialogState);
            networkService.ObserveToExecute<StopDialogCommand>(CleanDialogState);
        }

        private void OnConnectionStateChanged(ConnectionState state)
        {
            foreach (uint netId in state.Players.Where(id => !localPlayers.ContainsKey(id)))
            {
                if (networkFactory.Players.TryGetValue(netId, out NetworkIdentity identity) && identity.isOwned)
                {
                    GameInputActions actions = inputService.GetInput(netId);
                    if (actions != null)
                    {
                        localPlayers[netId] = new DialogLocalPlayer(netId, actions, networkService);
                    }
                }
            }

            foreach (uint netId in localPlayers.Keys.Where(id => !state.Players.Contains(id)).ToList())
            {
                localPlayers[netId].Dispose();
                localPlayers.Remove(netId);
            }
        }

        private async UniTask LoadDialogsAsync()
        {
            IList<DialogSequence> dialogs =
                await Addressables.LoadAssetsAsync<DialogSequence>(GlobalParams.DialogsAddressableGroup, null);

            dialogsMap = dialogs.ToDictionary(item => item.Id, item => item);
        }

        IDisposable IDialogServiceMediator.StartDialog(uint initiatorId, Guid dialogId)
        {
            StartDialogCommand command = new StartDialogCommand {DialogId = dialogId, InitiatorId = initiatorId};
            networkService.SendCommand(command);
            return Disposable.Create(() => networkService.SendCommand(new StopDialogCommand()));
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

            foreach (DialogLocalPlayer player in localPlayers.Values)
            {
                player.StartDialog();
            }
        }

        private void StopLocalDialog(StopDialogCommand command)
        {
            windowService.HideWindow(dialogWindowType.Id);
            inputService.SwitchToDefaultContext();

            foreach (DialogLocalPlayer player in localPlayers.Values)
            {
                player.StopDialog();
            }
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

            if (connectedPlayers.All(netId => readyPlayers.Contains(netId)))
            {
                replicaIndex++;
                readyPlayers.Clear();
                if (dialogsMap.TryGetValue(dialogState.DialogId, out DialogSequence sequence) &&
                    replicaIndex >= sequence.Replicas.Length)
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

        private class DialogLocalPlayer : IDisposable
        {
            private readonly uint netId;
            private readonly GameInputActions actions;
            private readonly INetworkService networkService;
            private bool isDialogActive;

            public DialogLocalPlayer(uint netId, GameInputActions actions, INetworkService networkService)
            {
                this.netId = netId;
                this.actions = actions;
                this.networkService = networkService;
            }

            public void StartDialog()
            {
                if (!isDialogActive)
                {
                    isDialogActive = true;
                    actions.DialogContext.Next.performed += OnNextPressed;
                }
            }

            public void StopDialog()
            {
                if (isDialogActive)
                {
                    isDialogActive = false;
                    actions.DialogContext.Next.performed -= OnNextPressed;
                }
            }

            private void OnNextPressed(UnityEngine.InputSystem.InputAction.CallbackContext context)
            {
                networkService.SendCommand(new ReadyDialogCommand { InitiatorId = netId });
            }

            public void Dispose()
            {
                StopDialog();
            }
        }
    }
}
