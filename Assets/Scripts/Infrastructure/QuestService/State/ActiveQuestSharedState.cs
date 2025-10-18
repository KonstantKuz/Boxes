using Infrastructure.Network.Abstract;
using MessagePack;

namespace Infrastructure.QuestService.State
{
    [MessagePackObject]
    public struct ActiveQuestSharedState : INetworkState
    {
        public static ActiveQuestSharedState Default => new(-1, -1);

        [Key(0)]
        public int QuestIndex { get; }

        [Key(1)]
        public int TaskIndex { get; }

        [Key(2)]
        public bool RestartRequired { get; }

        public ActiveQuestSharedState(int questIndex, int taskIndex, bool restartRequired = false)
        {
            QuestIndex = questIndex;
            TaskIndex = taskIndex;
            RestartRequired = restartRequired;
        }
    }
}
