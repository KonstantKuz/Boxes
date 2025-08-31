using R3;

namespace Infrastructure.QuestService.Abstract
{
    public interface ITask
    {
        ReadOnlyReactiveProperty<(string Title, string Description)> DisplayData { get; }
        ReadOnlyReactiveProperty<bool> IsDone { get; }
        void Start();
    }
}
