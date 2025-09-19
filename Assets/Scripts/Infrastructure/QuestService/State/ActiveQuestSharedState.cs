using Infrastructure.Network.Abstract;
using MessagePack;

namespace Infrastructure.QuestService.State
{
    [MessagePackObject]
    public struct ActiveQuestSharedState : INetworkState
    {
        public static ActiveQuestSharedState Default => new ActiveQuestSharedState(-1, -1);

        [Key(0)]
        public int QuestIndex { get; }

        [Key(1)]
        public int TaskIndex { get; }

        public ActiveQuestSharedState(int questIndex, int taskIndex)
        {
            QuestIndex = questIndex;
            TaskIndex = taskIndex;
        }
    }
}
