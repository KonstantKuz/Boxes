using UnityEngine;
using UnityEngine.Localization;

namespace Infrastructure.QuestService
{
    [CreateAssetMenu(fileName = nameof(TaskDescription), menuName = GlobalParams.QuestsPath + nameof(TaskDescription))]
    public class TaskDescription : ScriptableObject
    {
        [SerializeField]
        private LocalizedString title;

        [SerializeField]
        private LocalizedString description;

        public LocalizedString Title => title;
        public LocalizedString Description => description;
    }
}
