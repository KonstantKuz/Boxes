using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Infrastructure.Bootstrap;
using Infrastructure.QuestService.Abstract;
using R3;
using Reflex.Attributes;
using Reflex.Core;
using Reflex.Injectors;
using UnityEngine;

namespace Infrastructure.QuestService
{
    [Serializable]
    public class QuestService : IQuestService, IInitializable
    {
        private readonly ReactiveProperty<ITask> activeTask = new();

        [SerializeReference]
        private TaskSequence beginningTaskSequence;

        [SerializeField]
        private int currentQuestIndex;

        [SerializeField]
        private int currentTaskIndex;

        [SerializeReference]
        private List<TaskSequence> quests;

        private Container container;

        ReactiveProperty<ITask> IQuestService.ActiveTask => activeTask;

        [Inject]
        private void Construct(Container container)
        {
            this.container = container;
        }

        void IInitializable.Initialize()
        {
            foreach (TaskSequence quest in quests)
            {
                AttributeInjector.Inject(quest, container);
            }

            UniTask.Void(async () =>
            {
                await UniTask.Yield();
                StartCurrentQuest();
            });
        }

        private void StartCurrentQuest()
        {
            TaskSequence currentQuest = quests[currentQuestIndex];
            currentQuest.Start(currentTaskIndex);
            activeTask.Value = currentQuest;
            ((ITask) currentQuest).IsDone.Subscribe(OnCurrentQuestDone);
        }

        private void OnCurrentQuestDone(bool isDone)
        {
            if (isDone)
            {
                if (activeTask.Value is IDisposable disposable)
                {
                    disposable.Dispose();
                }

                currentQuestIndex++;
                if (currentQuestIndex < quests.Count)
                {
                    StartCurrentQuest();
                }
            }
        }
    }
}
