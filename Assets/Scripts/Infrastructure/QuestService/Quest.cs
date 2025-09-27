using Infrastructure.QuestService.Abstract;
using UnityEngine;

namespace Infrastructure.QuestService
{
    [CreateAssetMenu(fileName = nameof(Quest), menuName = GlobalParams.QuestsPath + nameof(Quest))]
    public class Quest : ScriptableObject
    {
        [SerializeReference]
        private TaskSequence taskSequence;

        public TaskSequence TaskSequence => taskSequence;
    }
}
