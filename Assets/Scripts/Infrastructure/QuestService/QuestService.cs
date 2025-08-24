using System;
using Configuration.QuestTasks.Beginning;
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
    public class QuestService : IQuestService, IInitializable
    {
        [SerializeReference]
        private TaskSequence beginningTaskSequence;

        private Container container;
        private readonly ReactiveProperty<ITask> activeTask = new();

        ReactiveProperty<ITask> IQuestService.ActiveTask => activeTask;

        [Inject]
        private void Construct(Container container)
        {
            this.container = container;
        }

        void IInitializable.Initialize()
        {
            UniTask.Void(async () =>
            {
                AttributeInjector.Inject(beginningTaskSequence, container);
                await UniTask.Yield();
                ((ITask)beginningTaskSequence).Start();
                activeTask.Value = beginningTaskSequence;
                ((ITask) beginningTaskSequence).IsDone.Subscribe(CleanActiveTask);
            });
        }

        private void CleanActiveTask(bool isDone)
        {
            if (isDone)
            {
                if (activeTask.Value is IDisposable disposable)
                {
                    disposable?.Dispose();
                }

                activeTask.Value = null;
            }
        }
    }
}
