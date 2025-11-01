using System;
using Infrastructure.QuestService.Abstract;
using R3;
using Reflex.Attributes;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;

namespace Infrastructure.QuestService.Components
{
    public class QuestDependencyHandler : MonoBehaviour
    {
        [Serializable]
        public struct Dependency
        {
            [Required]
            [SerializeField]
            private Quest targetQuest;

            [SerializeField]
            private UnityEvent<bool> onTargetQuestIsActiveChanged;

            public Quest TargetQuest => targetQuest;
            public UnityEvent<bool> OnTargetQuestIsActiveChanged => onTargetQuestIsActiveChanged;
        }

        [SerializeField]
        [ListDrawerSettings(ShowIndexLabels = true, ShowFoldout = true)]
        private Dependency[] dependencies;

        private IQuestService questService;
        private IDisposable questSubscription;
        private Quest currentActiveQuest;

        [Inject]
        private void Construct(IQuestService questService)
        {
            this.questService = questService;
        }

        private void OnEnable()
        {
            if (questService != null)
            {
                questSubscription = questService.ActiveQuest.Subscribe(OnActiveQuestChanged);
            }
        }

        private void OnDisable()
        {
            questSubscription?.Dispose();
        }

        private void OnActiveQuestChanged(Quest newActiveQuest)
        {
            Quest previousQuest = currentActiveQuest;
            currentActiveQuest = newActiveQuest;

            foreach (Dependency dependency in dependencies)
            {
                if (dependency.TargetQuest == null)
                {
                    continue;
                }

                if (previousQuest == dependency.TargetQuest && newActiveQuest != dependency.TargetQuest)
                {
                    dependency.OnTargetQuestIsActiveChanged?.Invoke(false);
                }

                if (newActiveQuest == dependency.TargetQuest && previousQuest != dependency.TargetQuest)
                {
                    dependency.OnTargetQuestIsActiveChanged?.Invoke(true);
                }
            }
        }

        [Button("Force Update Current State")]
        [PropertySpace(10)]
        public void ForceUpdateState()
        {
            if (currentActiveQuest != null)
            {
                foreach (Dependency dependency in dependencies)
                {
                    if (dependency.TargetQuest == currentActiveQuest)
                    {
                        dependency.OnTargetQuestIsActiveChanged?.Invoke(true);
                    }
                    else if (dependency.TargetQuest != null)
                    {
                        dependency.OnTargetQuestIsActiveChanged?.Invoke(false);
                    }
                }
            }
        }
    }
}
