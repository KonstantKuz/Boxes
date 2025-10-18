using Infrastructure.Bootstrap;
using R3;

namespace Infrastructure.QuestService.Abstract
{
    public interface IQuestService : IPostBuildInjectable
    {
        ReactiveProperty<ITask> ActiveTask { get; }
        void RegisterQuestRoot(QuestRoot questRoot);
        void UnregisterQuestRoot(QuestRoot questRoot);
        void RestartQuest();
    }
}
