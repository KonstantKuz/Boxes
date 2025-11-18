using System;
using Infrastructure;
using Infrastructure.NavigationService;
using Infrastructure.QuestService;
using Infrastructure.QuestService.Abstract;
using Infrastructure.World;
using R3;
using R3.Triggers;
using Reflex.Attributes;
using UnityEngine;

namespace Configuration.QuestTasks.Shared
{
    [Serializable]
    public class NavigationTask : TaskBase, IDisposable
    {
        [SerializeField]
        private TaskDescription description;

        [WorldObjectId]
        [SerializeField]
        private string targetId;

        private INavigationService navigationService;
        private IWorldService worldService;

        private IDisposable triggerDisposable;

        [Inject]
        private void Construct(INavigationService navigationService, IWorldService worldService)
        {
            this.navigationService = navigationService;
            this.worldService = worldService;
        }

        public override void Start()
        {
            DisplayData.Value = (description.Title.GetLocalizedString(), description.Description.GetLocalizedString());
            if (!worldService.TryGetById(targetId, out IWorldObject target))
            {
                this.Log(LogType.Error, $"Target not found with id {targetId}");
                return;
            }

            navigationService.SetActiveTarget(target.Value.transform);
            triggerDisposable = target.Value.transform.OnTriggerEnterAsObservable()
                .Where(item => item.CompareTag(GlobalParams.PlayerTag))
                .Subscribe(_ => IsDone.Value = true);
        }

        void IDisposable.Dispose()
        {
            navigationService.SetActiveTarget(null);
            triggerDisposable?.Dispose();
        }
    }
}
