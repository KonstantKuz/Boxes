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
    public class TaskBag : TaskBase, IDisposable
    {
        [SerializeReference]
        private List<ITask> tasks;

        private IDisposable displayDataDisposable;
        private IDisposable isDoneDisposable;

        [Inject]
        private void Construct(Container container)
        {
            foreach (ITask task in tasks)
            {
                AttributeInjector.Inject(task, container);
            }
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

            displayDataDisposable?.Dispose();
            isDoneDisposable?.Dispose();
        }
    }
}
