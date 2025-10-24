using Infrastructure.Bootstrap;
using R3;

namespace Infrastructure.QuestService.Abstract
{
    public interface IQuestService : IPostBuildInjectable
    {
        ReadOnlyReactiveProperty<Quest> ActiveQuest { get; }
        ReactiveProperty<ITask> ActiveTask { get; }
        void RegisterQuestRoot(QuestRoot questRoot);
        void UnregisterQuestRoot(QuestRoot questRoot);
        void RestartQuest();
    }
}
