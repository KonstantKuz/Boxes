using System;
using Infrastructure.QuestService.Abstract;
using R3;
using Reflex.Attributes;
using Reflex.Core;
using Reflex.Injectors;
using UnityEngine;

namespace Configuration.QuestTasks.Shared
{
    [Serializable]
    public class DelayedTask : TaskBase, IDisposable
    {
        [SerializeField]
        private float seconds;

        [SerializeField]
        private bool isAwaitRequired;

        [SerializeReference]
        private ITask task;

        private CompositeDisposable disposable;

        [Inject]
        private void Construct(Container container)
        {
            AttributeInjector.Inject(task, container);
        }

        public override void Start()
        {
            if (!isAwaitRequired)
            {
                IsDone.Value = true;
            }

            disposable = new CompositeDisposable();

            Observable
                .Timer(TimeSpan.FromSeconds(seconds), UnityTimeProvider.Update)
                .Subscribe(_ => StartNestedTask())
                .AddTo(disposable);
        }

        private void StartNestedTask()
        {
            task.Start();
            task.IsDone.Subscribe(value => IsDone.Value = value).AddTo(disposable);

            if (task is IDisposable taskDisposable)
            {
                disposable.Add(taskDisposable);
            }
        }

        void IDisposable.Dispose()
        {
            disposable?.Dispose();
        }
    }
}
