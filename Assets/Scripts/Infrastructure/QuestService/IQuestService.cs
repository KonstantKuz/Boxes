using Infrastructure.Bootstrap;
using R3;

namespace Infrastructure.QuestService
{
    public interface IQuestService : IPostBuildInjectable
    {
        ReactiveProperty<ITask> ActiveTask { get; }
    }
}
