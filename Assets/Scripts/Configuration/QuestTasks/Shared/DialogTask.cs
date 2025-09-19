using System;
using Infrastructure.DialogService;
using Infrastructure.DialogService.Abstract;
using Infrastructure.DialogService.Command;
using Infrastructure.Network.Abstract;
using Infrastructure.QuestService.Abstract;
using Infrastructure.World;
using Reflex.Attributes;
using UnityEngine;

namespace Configuration.QuestTasks.Shared
{
    [Serializable]
    public class DialogTask : TaskBase, IDisposable
    {
        [SerializeField]
        private DialogSequence dialogSequence;

        [SerializeField]
        private string cameraTargetId;

        private IDisposable disposable;
        private INetworkService networkService;
        private IDialogService dialogService;
        private IWorldService worldService;
        private IWorldObject cameraTarget;

        [Inject]
        private void Construct(INetworkService networkService, IDialogService dialogService, IWorldService worldService)
        {
            this.networkService = networkService;
            this.dialogService = dialogService;
            this.worldService = worldService;
        }

        public override void Start()
        {
            if (worldService.TryGetById(cameraTargetId, out cameraTarget))
            {
                cameraTarget.Value.SetActive(true);
            }

            dialogService.StartDialog(0, dialogSequence.Id);

            disposable = networkService.ObserveToReact<StopDialogCommand>(OnDialogStop);
        }

        private void OnDialogStop(StopDialogCommand context)
        {
            cameraTarget?.Value.SetActive(false);
            IsDone.Value = true;
        }

        void IDisposable.Dispose()
        {
            disposable?.Dispose();
        }
    }
}
