using System;
using System.Collections.Generic;
using System.Linq;
using R3;

namespace Infrastructure.QuestService.Abstract
{
    public class TaskSequence : ITask, IDisposable
    {
        private List<ITask> tasks;
        private ReactiveProperty<bool> isDone;
        private IDisposable activeTaskDisposable;
        private int currentIndex;

        Observable<(string Title, string Description)> ITask.DisplayData =>
            Observable.Merge(tasks.Select(item => item.DisplayData));

        ReadOnlyReactiveProperty<bool> ITask.IsDone => Observable
            .CombineLatest(tasks.Select(item => item.IsDone))
            .Select(values => values.All(value => value))
            .ToReadOnlyReactiveProperty();

        public void Build(List<ITask> tasks)
        {
            this.tasks = tasks;
        }

        public void Start()
        {
            if (tasks == null || tasks.Count == 0)
            {
                return;
            }

            isDone = Observable
                .CombineLatest(tasks.Select(item => item.IsDone))
                .Select(values => values.All(value => value))
                .ToBindableReactiveProperty();

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
                    ((IDisposable) task).Dispose();
                    activeTaskDisposable?.Dispose();
                    StartTaskAt(index + 1);
                });

            task.Start();
        }

        void IDisposable.Dispose()
        {
            IEnumerable<IDisposable> disposables =
                tasks.Select(task => task as IDisposable).Where(disposable => disposable != null);

            foreach (IDisposable task in disposables)
            {
                task.Dispose();
            }
        }
    }
}
