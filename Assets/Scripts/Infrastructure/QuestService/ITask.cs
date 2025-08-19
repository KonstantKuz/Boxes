using R3;

namespace Infrastructure.QuestService
{
    public interface ITask
    {
        Observable<(string Title, string Description)> DisplayData { get; }
        ReadOnlyReactiveProperty<bool> IsDone { get; }
    }
}