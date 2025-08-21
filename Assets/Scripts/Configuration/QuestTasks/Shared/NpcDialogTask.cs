using System;
using Gameplay.Interactable.Dialog;
using Infrastructure.DialogService.Command;
using Infrastructure.Network.Abstract;
using Infrastructure.QuestService.Abstract;
using Reflex.Attributes;
using UnityEngine;

namespace Configuration.QuestTasks.Shared
{
    [Serializable]
    public class NpcDialogTask : TaskBase, IDisposable
    {
        [SerializeField]
        private DialogOwner dialogOwner;

        private IDisposable disposable;
        private INetworkService networkService;

        [Inject]
        private void Construct(INetworkService networkService)
        {
            this.networkService = networkService;
        }

        public override void Start()
        {
            dialogOwner.gameObject.SetActive(true);
            dialogOwner.StartDialog(0);

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
