using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Infrastructure.Bootstrap;
using Infrastructure.Network.Abstract;
using Infrastructure.Network.State;
using Infrastructure.QuestService.Abstract;
using Infrastructure.QuestService.State;
using R3;
using Reflex.Attributes;
using Reflex.Core;
using Reflex.Injectors;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Infrastructure.QuestService
{
    [Serializable]
    public partial class QuestService : IQuestService, IInitializable, IDisposable
    {
        private readonly ReactiveProperty<ITask> activeTask = new();
        private readonly ReactiveProperty<Quest> activeQuest = new();

        [SerializeField]
        private int currentQuestIndex;

        [SerializeField]
        private int currentTaskIndex;

        [SerializeField]
        private List<Quest> quests;

        private Container container;
        private INetworkStateHolder<ActiveQuestSharedState> questStateHolder;
        private INetworkManager networkManager;
        private INetworkFactory networkFactory;

        private Dictionary<Quest, QuestRoot> roots;

        ReadOnlyReactiveProperty<Quest> IQuestService.ActiveQuest => activeQuest;
        ReactiveProperty<ITask> IQuestService.ActiveTask => activeTask;

        [Inject]
        private void Construct(
            Container container,
            INetworkStateHolder<ActiveQuestSharedState> questStateHolder,
            INetworkManager networkManager,
            INetworkFactory networkFactory,
            INetworkStateHolder<ConnectionState> connectionStateHolder
        )
        {
            this.container = container;
            this.questStateHolder = questStateHolder;
            this.networkManager = networkManager;
            this.networkFactory = networkFactory;


            roots = new Dictionary<Quest, QuestRoot>();
        }

        void IInitializable.Initialize()
        {
            UniTask.Void(async () =>
            {
                await UniTask.WaitUntil(() => networkFactory.LocalPlayer != null && networkManager.IsClientReady);

                questStateHolder.Subscribe(UpdateLocalState);

                if (!networkManager.IsServer)
                {
                    return;
                }

                questStateHolder.WriteState(new ActiveQuestSharedState(currentQuestIndex, currentTaskIndex));
            });
        }

        void IQuestService.RegisterQuestRoot(QuestRoot questRoot)
        {
            roots.Add(questRoot.Quest, questRoot);
        }

        void IQuestService.UnregisterQuestRoot(QuestRoot questRoot)
        {
            roots.Remove(questRoot.Quest);
        }

        void IQuestService.RestartQuest()
        {
            if (!networkManager.IsServer)
            {
                return;
            }

            currentTaskIndex = 0;
            questStateHolder.WriteState(new ActiveQuestSharedState(currentQuestIndex, currentTaskIndex, true));
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

            if (currentQuestIndex < quests.Count &&
                currentTaskIndex < quests[currentQuestIndex].TaskSequence.TaskCount)
            {
                StartCurrentQuest().Forget();
            }
        }

        private async UniTask StartCurrentQuest()
        {
            if (activeTask.Value is IDisposable disposable)
            {
                disposable.Dispose();
            }

            if (roots.TryGetValue(quests[currentQuestIndex], out QuestRoot root))
            {
                await root.ResetState();

                if (questStateHolder.GetState().RestartRequired)
                {
                    if (networkFactory.LocalPlayer.TryGetComponent(out Rigidbody rigidbody))
                    {
                        rigidbody.velocity = Vector3.zero;
                        rigidbody.angularVelocity = Vector3.zero;
                        rigidbody.MovePosition(root.Spawn.position);
                        rigidbody.MoveRotation(root.Spawn.rotation);
                    }
                }
            }

            Quest quest = Object.Instantiate(quests[currentQuestIndex]);
            TaskSequence currentQuest = quest.TaskSequence;
            AttributeInjector.Inject(currentQuest, container);
            currentQuest.Start(currentTaskIndex);

            activeQuest.Value = quests[currentQuestIndex];
            activeTask.Value = currentQuest;

            ((ITask) currentQuest).IsDone.Subscribe(OnCurrentQuestDone);

            this.Log(LogType.Log, $"Start quest {quests[currentQuestIndex].name}");
            currentQuest.ActiveTask.Subscribe(
                task => this.Log(LogType.Log, $"Start task {task.GetType()}")
            );
        }

        private void OnCurrentQuestDone(bool isDone)
        {
            if (!networkManager.IsServer)
            {
                return;
            }

            if (isDone)
            {
                currentQuestIndex++;
                currentTaskIndex = 0;
                questStateHolder.WriteState(new ActiveQuestSharedState(currentQuestIndex, currentTaskIndex));
            }
        }
    }
}
