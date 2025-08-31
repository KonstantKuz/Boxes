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
        private TaskConfig config;

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
            DisplayData.Value = (config.Title.GetLocalizedString(), config.Description.GetLocalizedString());
            if (!worldService.TryGetById(Guid.Parse(targetId), out IWorldObject target))
            {
                this.Log(LogType.Error, $"Target not found with id {targetId}");
                return;
            }

            navigationService.SetActiveTarget(target.Value.transform);
            triggerDisposable = target.Value.transform.OnTriggerEnterAsObservable()
                .Where(item => item.CompareTag("Player"))
                .Subscribe(_ => IsDone.Value = true);
        }

        void IDisposable.Dispose()
        {
            navigationService.SetActiveTarget(null);
            triggerDisposable?.Dispose();
        }
    }
}
