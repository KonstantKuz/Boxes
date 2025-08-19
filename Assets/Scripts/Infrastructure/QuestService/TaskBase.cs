using R3;

namespace Infrastructure.QuestService
{
    public class TaskBase : ITask
    {
        protected readonly ReactiveProperty<(string, string)>  DisplayData = new();
        protected readonly ReactiveProperty<bool> IsDone = new();

        Observable<(string Title, string Description)> ITask.DisplayData => DisplayData;
        ReadOnlyReactiveProperty<bool> ITask.IsDone => IsDone;
    }
}