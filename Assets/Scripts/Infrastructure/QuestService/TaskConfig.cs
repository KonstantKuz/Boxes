using System;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Localization;

namespace Infrastructure.QuestService
{
    [CreateAssetMenu(fileName = nameof(TaskConfig), menuName = GlobalParams.TaskConfigPath + nameof(TaskConfig))]
    public class TaskConfig : ScriptableObject
    {
        [SerializeField]
        private string guid;

        [SerializeField]
        private LocalizedString title;

        [SerializeField]
        private LocalizedString description;

        public Guid Id => Guid.Parse(guid);
        public LocalizedString Title => title;
        public LocalizedString Description => description;

        [Button]
        private void GenerateGuid()
        {
            guid = Guid.NewGuid().ToString();
        }
    }
}
