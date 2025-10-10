using Infrastructure.QuestService.Abstract;
using Reflex.Attributes;
using UnityEngine;

namespace Infrastructure.QuestService
{
    public class QuestRoot : MonoBehaviour
    {
        [SerializeField]
        private Quest quest;

        private IQuestService questService;

        public Quest Quest => quest;

        [Inject]
        private void Construct(IQuestService questService)
        {
            this.questService = questService;
        }

        private void OnEnable()
        {
            questService.RegisterQuestRoot(this);
        }

        private void OnDisable()
        {
            questService.UnregisterQuestRoot(this);
        }
    }
}
