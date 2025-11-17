using System;
using Infrastructure.DialogService;
using Infrastructure.DialogService.Abstract;
using Infrastructure.DialogService.Command;
using Infrastructure.Network.Abstract;
using Infrastructure.QuestService.Abstract;
using Infrastructure.World;
using R3;
using Reflex.Attributes;
using UnityEngine;

namespace Configuration.QuestTasks.Shared
{
    [Serializable]
    public class DialogTask : TaskBase, IDisposable
    {
        [SerializeField]
        private DialogSequence dialogSequence;

        [WorldObjectId]
        [SerializeField]
        private string cameraTargetId;

        private CompositeDisposable disposable;
        private INetworkService networkService;
        private INetworkManager networkManager;
        private IDialogService dialogService;
        private IWorldService worldService;
        private IWorldObject cameraTarget;

        [Inject]
        private void Construct(
            INetworkService networkService,
            INetworkManager networkManager,
            IDialogService dialogService,
            IWorldService worldService
        )
        {
            this.networkService = networkService;
            this.networkManager = networkManager;
            this.dialogService = dialogService;
            this.worldService = worldService;
        }

        public override void Start()
        {
            if (worldService.TryGetById(cameraTargetId, out cameraTarget))
            {
                cameraTarget.Value.SetActive(true);
            }

            disposable = new CompositeDisposable();

            networkService.ObserveToReact<StopDialogCommand>(OnDialogStop).AddTo(disposable);

            if (networkManager.IsServer)
            {
                dialogService.StartDialog(0, dialogSequence.Id).AddTo(disposable);
            }
        }

        private void OnDialogStop(StopDialogCommand context)
        {
            cameraTarget?.Value.SetActive(false);
            IsDone.Value = true;
        }

        void IDisposable.Dispose()
        {
            cameraTarget?.Value.SetActive(false);
            disposable?.Dispose();
        }
    }
}
