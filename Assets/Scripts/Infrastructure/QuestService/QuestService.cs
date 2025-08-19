using System;
using Infrastructure.Bootstrap;
using R3;
using Reflex.Attributes;
using Reflex.Core;
using UnityEngine;

namespace Infrastructure.QuestService
{
    public class QuestService : IQuestService, IInitializable
    {
        [SerializeField]
        private TaskConfig testConfig;

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
            PassBallTask passBallTask = container.Resolve<PassBallTask>();
            passBallTask.Initialize(testConfig);
            activeTask.Value = passBallTask;
            ((ITask) passBallTask).IsDone.Subscribe(CleanActiveTask);
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
