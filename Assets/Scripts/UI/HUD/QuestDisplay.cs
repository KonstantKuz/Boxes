using System;
using Infrastructure.Bootstrap;
using Infrastructure.QuestService;
using Infrastructure.QuestService.Abstract;
using R3;
using Reflex.Attributes;
using UnityEngine;
using UnityEngine.Events;

namespace UI.HUD
{
    public class QuestDisplay : MonoBehaviour, IInitializable, IDisposable
    {
        [SerializeField]
        private UnityEvent<string> onTitleChanged;

        [SerializeField]
        private UnityEvent<string> onDescriptionChanged;

        private IQuestService questService;
        private IDisposable questSubscription;

        [Inject]
        private void Construct(IQuestService questService)
        {
            this.questService = questService;
        }

        void IInitializable.Initialize()
        {
            questSubscription = questService.ActiveTask
                .Where(task => task != null)
                .SelectMany(task => task?.DisplayData)
                .CombineLatest(questService.ActiveTask, (displayData, task) => task == null ? (string.Empty, string.Empty) : displayData)
                .Subscribe(UpdateDisplay);
        }

        private void UpdateDisplay((string title, string description) data)
        {
            onTitleChanged.Invoke(data.title);
            onDescriptionChanged.Invoke(data.description);
        }

        void IDisposable.Dispose()
        {
            questSubscription?.Dispose();
        }
    }
}
