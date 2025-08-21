using System;
using Infrastructure.NavigationService;
using Infrastructure.QuestService;
using Infrastructure.QuestService.Abstract;
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
        private Transform targetTrigger;

        private INavigationService navigationService;
        private IDisposable triggerDisposable;

        [Inject]
        private void Construct(INavigationService navigationService)
        {
            this.navigationService = navigationService;
        }

        public override void Start()
        {
            DisplayData.Value = (config.Title.GetLocalizedString(), config.Description.GetLocalizedString());
            navigationService.SetActiveTarget(targetTrigger);
            triggerDisposable = targetTrigger.OnTriggerEnterAsObservable()
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
