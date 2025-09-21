using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Infrastructure.Bootstrap;
using Infrastructure.Network.Abstract;
using Infrastructure.QuestService.Abstract;
using Infrastructure.QuestService.State;
using R3;
using Reflex.Attributes;
using Reflex.Core;
using Reflex.Injectors;
using UnityEngine;

namespace Infrastructure.QuestService
{
    [Serializable]
    public class QuestService : IQuestService, IInitializable, IDisposable
    {
        private readonly ReactiveProperty<ITask> activeTask = new();

        [SerializeField]
        private int currentQuestIndex;

        [SerializeField]
        private int currentTaskIndex;

        [SerializeField]
        private List<Quest> quests;

        private INetworkStateHolder<ActiveQuestSharedState> questStateHolder;
        private INetworkManager networkManager;

        ReactiveProperty<ITask> IQuestService.ActiveTask => activeTask;

        [Inject]
        private void Construct(
            Container container,
            INetworkStateHolder<ActiveQuestSharedState> questStateHolder,
            INetworkManager networkManager
        )
        {
            this.questStateHolder = questStateHolder;
            this.networkManager = networkManager;

            foreach (Quest quest in quests)
            {
                AttributeInjector.Inject(quest.TaskSequence, container);
            }
        }

        void IInitializable.Initialize()
        {
            questStateHolder.Subscribe(UpdateLocalState);

            if (!networkManager.IsServer)
            {
                return;
            }

            UniTask.Void(async () =>
            {
                await UniTask.Yield();
                await UniTask.WaitForFixedUpdate();
                questStateHolder.WriteState(new ActiveQuestSharedState(currentQuestIndex, currentTaskIndex));
            });
        }

        void IDisposable.Dispose()
        {
            if (activeTask.Value is IDisposable disposable)
            {
                disposable.Dispose();
            }

            activeTask?.Dispose();
        }

        private void UpdateLocalState(ActiveQuestSharedState state)
        {
            if (Equals(state, ActiveQuestSharedState.Default))
            {
                return;
            }

            currentQuestIndex = state.QuestIndex;
            currentTaskIndex = state.TaskIndex;

            if (currentQuestIndex < quests.Count && currentTaskIndex < quests[currentQuestIndex].TaskSequence.TaskCount)
            {
                StartCurrentQuest();
            }
        }

        private void StartCurrentQuest()
        {
            TaskSequence currentQuest = quests[currentQuestIndex].TaskSequence;
            currentQuest.Start(currentTaskIndex);
            activeTask.Value = currentQuest;
            ((ITask) currentQuest).IsDone.Subscribe(OnCurrentQuestDone);
        }

        private void OnCurrentQuestDone(bool isDone)
        {
            if (!networkManager.IsServer)
            {
                return;
            }

            if (isDone)
            {
                if (activeTask.Value is IDisposable disposable)
                {
                    disposable.Dispose();
                }

                currentQuestIndex++;
                if (currentQuestIndex < quests.Count)
                {
                    questStateHolder.WriteState(new ActiveQuestSharedState(currentQuestIndex, currentTaskIndex));
                }
            }
        }
    }
}
