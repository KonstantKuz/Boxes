using R3;

namespace Infrastructure.QuestService.Abstract
{
    public class TaskBase : ITask
    {
        protected readonly ReactiveProperty<(string, string)>  DisplayData = new();
        protected readonly ReactiveProperty<bool> IsDone = new();

        ReadOnlyReactiveProperty<(string Title, string Description)> ITask.DisplayData => DisplayData;
        ReadOnlyReactiveProperty<bool> ITask.IsDone => IsDone;

        public virtual void Start()
        {
        }
    }
}
