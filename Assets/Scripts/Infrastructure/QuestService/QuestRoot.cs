using System.Linq;
using Cysharp.Threading.Tasks;
using Infrastructure.Components;
using Infrastructure.Network.Components;
using Infrastructure.QuestService.Abstract;
using Mirror;
using Reflex.Attributes;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Infrastructure.QuestService
{
    public class QuestRoot : MonoBehaviour
    {
        [InlineEditor]
        [SerializeField]
        private Quest quest;

        private IQuestService questService;
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
        }

        private void OnDisable()
        {
            questService.UnregisterQuestRoot(this);
        }

        public async UniTask ResetState()
        {
            if (initialStateHelpers == null || networkInitialStateHelpers == null)
            {
                initialStateHelpers = GetComponentsInChildren<InitialStateHelper>(true);
                networkInitialStateHelpers = GetComponentsInChildren<NetworkInitialStateHelper>(true);
            }

            await UniTask.WaitWhile(() => networkInitialStateHelpers.Any(item => item.netIdentity == null));

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
