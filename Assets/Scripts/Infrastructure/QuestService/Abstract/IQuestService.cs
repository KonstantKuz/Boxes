using System.Collections.Generic;
using System.Collections.ObjectModel;
using Infrastructure.Bootstrap;
using R3;

namespace Infrastructure.QuestService.Abstract
{
    public interface IQuestService : IPostBuildInjectable
    {
        ReadOnlyCollection<Quest> Quests { get; }
        ReadOnlyReactiveProperty<Quest> ActiveQuest { get; }
        ReactiveProperty<ITask> ActiveTask { get; }
        void RegisterQuestRoot(QuestRoot questRoot);
        void UnregisterQuestRoot(QuestRoot questRoot);
        void RestartQuest();
    }
}
