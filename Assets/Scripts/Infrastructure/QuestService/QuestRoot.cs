using System;
using System.Linq;
using Cysharp.Threading.Tasks;
using Infrastructure.Components;
using Infrastructure.Network.Components;
using Infrastructure.QuestService.Abstract;
using Mirror;
using R3;
using Reflex.Attributes;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;

namespace Infrastructure.QuestService
{
    public class QuestRoot : MonoBehaviour
    {
        [SerializeField]
        private UnityEvent<bool> OnQuestIsActiveChanged;

        [InlineEditor]
        [SerializeField]
        private Quest quest;

        private IQuestService questService;
        private IDisposable questSubscription;
        private NetworkStartPosition startPosition;

        private InitialStateHelper[] initialStateHelpers;
        private NetworkInitialStateHelper[] networkInitialStateHelpers;

        public Transform Spawn
        {
            get
            {
                startPosition ??= GetComponentInChildren<NetworkStartPosition>(true);
                return startPosition.transform;
            }
        }

        public Quest Quest => quest;

        [Inject]
        private void Construct(IQuestService questService)
        {
            this.questService = questService;
        }

        private void OnEnable()
        {
            questService.RegisterQuestRoot(this);
            questSubscription = questService.ActiveQuest.Subscribe(OnActiveQuestChanged);
        }

        private void OnDisable()
        {
            questService.UnregisterQuestRoot(this);
            questSubscription?.Dispose();
        }

        private void OnActiveQuestChanged(Quest quest)
        {
            OnQuestIsActiveChanged.Invoke(this.quest == quest);
        }

        public async UniTask ResetState()
        {
            if (initialStateHelpers == null || networkInitialStateHelpers == null)
            {
                initialStateHelpers = GetComponentsInChildren<InitialStateHelper>(true);
                networkInitialStateHelpers = GetComponentsInChildren<NetworkInitialStateHelper>(true);
            }

            await UniTask.WaitWhile(() =>
                !NetworkClient.active ||
                !NetworkClient.ready ||
                networkInitialStateHelpers.Any(item => item.netIdentity == null)
            );

            foreach (InitialStateHelper stateHelper in initialStateHelpers)
            {
                stateHelper.ResetState();
            }

            foreach (NetworkInitialStateHelper stateHelper in networkInitialStateHelpers)
            {
                stateHelper.CmdResetState();
            }
        }
    }
}
