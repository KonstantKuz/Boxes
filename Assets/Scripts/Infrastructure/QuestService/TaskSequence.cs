using System.Collections.Generic;
using System.Linq;
using R3;

namespace Infrastructure.QuestService
{
    public class TaskSequence : ITask
    {
        private List<ITask> tasks;

        Observable<(string Title, string Description)> ITask.DisplayData =>
            Observable.Merge(tasks.Select(item => item.DisplayData));

        ReadOnlyReactiveProperty<bool> ITask.IsDone => Observable
            .CombineLatest(tasks.Select(item => item.IsDone))
            .Select(values => values.All(value => value))
            .ToReadOnlyReactiveProperty();

        public void Initialize(List<ITask> tasks)
        {
            this.tasks = tasks;
        }
    }
}