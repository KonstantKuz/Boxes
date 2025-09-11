using Infrastructure.QuestService.Abstract;
using UnityEngine;

namespace Infrastructure.QuestService
{
    public class Quest : MonoBehaviour
    {
        [SerializeReference]
        private TaskSequence taskSequence;

        public TaskSequence TaskSequence => taskSequence;
    }
}
