using System;
using Infrastructure.DialogService;
using Infrastructure.DialogService.Abstract;
using Infrastructure.DialogService.Command;
using Infrastructure.Network.Abstract;
using Infrastructure.QuestService.Abstract;
using Reflex.Attributes;
using UnityEngine;

namespace Configuration.QuestTasks.Shared
{
    [Serializable]
    public class DialogTask : TaskBase, IDisposable
    {
        [SerializeField]
        private DialogSequence dialogSequence;

        private IDisposable disposable;
        private INetworkService networkService;
        private IDialogService dialogService;

        [Inject]
        private void Construct(INetworkService networkService, IDialogService dialogService)
        {
            this.networkService = networkService;
            this.dialogService = dialogService;
        }

        public override void Start()
        {
            dialogService.StartDialog(0, dialogSequence.Id);

            disposable = networkService.ObserveToReact<StopDialogCommand>(OnDialogStop);
        }

        private void OnDialogStop(StopDialogCommand context)
        {
            IsDone.Value = true;
        }

        void IDisposable.Dispose()
        {
            disposable?.Dispose();
        }
    }
}
