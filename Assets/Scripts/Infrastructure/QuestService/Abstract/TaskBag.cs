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
    public class TaskBag : ITask, IDisposable
    {
        [SerializeReference]
        private List<ITask> tasks;

        Observable<(string Title, string Description)> ITask.DisplayData =>
            Observable.Merge(tasks.Select(item => item.DisplayData));

        ReadOnlyReactiveProperty<bool> ITask.IsDone => Observable
            .CombineLatest(tasks.Select(item => item.IsDone))
            .Select(values => values.All(value => value))
            .ToReadOnlyReactiveProperty();

        [Inject]
        private void Construct(Container container)
        {
            foreach (ITask task in tasks)
            {
                AttributeInjector.Inject(task, container);
            }
        }

        public void Start()
        {
            if (tasks == null || tasks.Count == 0)
            {
                return;
            }

            foreach (ITask task in tasks)
            {
                task.Start();
            }
        }

        void IDisposable.Dispose()
        {
            IEnumerable<IDisposable> disposables =
                tasks.Select(task => task as IDisposable).Where(disposable => disposable != null);

            foreach (IDisposable disposable in disposables)
            {
                disposable.Dispose();
            }
        }
    }
}
