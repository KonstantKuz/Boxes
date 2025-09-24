using System;
using System.Collections.Generic;
using System.Linq;
using R3;
using Reflex.Attributes;
using Reflex.Core;
using Reflex.Injectors;
using UnityEngine;

namespace Infrastructure.QuestService.Abstract
{
    [Serializable]
    public class TaskSequence : TaskBase, IDisposable
    {
        [SerializeReference]
        private List<ITask> tasks;

        private ReactiveProperty<ITask> activeTask;
        private IDisposable activeTaskDisposable;
        private int currentIndex;

        private IDisposable displayDataDisposable;
        private IDisposable isDoneDisposable;

        public int TaskCount => tasks.Count;
        public List<ITask> Tasks => tasks;
        public ReadOnlyReactiveProperty<ITask> ActiveTask => activeTask;

        [Inject]
        private void Construct(Container container)
        {
            activeTask = new ReactiveProperty<ITask>();

            foreach (ITask task in tasks)
            {
                AttributeInjector.Inject(task, container);
            }
        }

        public void Start(int taskIndex)
        {
            currentIndex = taskIndex;
            ((ITask) this).Start();
        }

        public override void Start()
        {
            if (tasks == null || tasks.Count == 0)
            {
                return;
            }

            displayDataDisposable = Observable
                .Merge(tasks.Select(item => item.DisplayData))
                .Subscribe(value => DisplayData.Value = value);

            isDoneDisposable = Observable
                .CombineLatest(tasks.Select(item => item.IsDone))
                .Select(values => values.All(value => value))
                .Subscribe(value => IsDone.Value = value);

            StartTaskAt(currentIndex);
        }

        private void StartTaskAt(int index)
        {
            if (index >= tasks.Count)
            {
                return;
            }

            ITask task = tasks[index];
            currentIndex = index;

            activeTaskDisposable?.Dispose();
            activeTaskDisposable = task.IsDone
                .Where(done => done)
                .Take(1)
                .Subscribe(_ =>
                {
                    if (task is IDisposable disposable)
                    {
                        disposable.Dispose();
                    }

                    activeTaskDisposable?.Dispose();
                    StartTaskAt(index + 1);
                });

            task.Start();
            activeTask.Value = task;
        }

        void IDisposable.Dispose()
        {
            IEnumerable<IDisposable> disposables =
                tasks.Select(task => task as IDisposable).Where(disposable => disposable != null);

            foreach (IDisposable task in disposables)
            {
                task.Dispose();
            }

            displayDataDisposable?.Dispose();
            isDoneDisposable?.Dispose();
        }
    }
}
