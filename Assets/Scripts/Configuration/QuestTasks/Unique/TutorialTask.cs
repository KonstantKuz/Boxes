using System;
using System.Collections.Generic;
using System.Linq;
using Gameplay.Interactable.BallInteraction.Abstract;
using Gameplay.Interactable.BallInteraction.State;
using Infrastructure.Network.Abstract;
using Infrastructure.Network.State;
using Infrastructure.QuestService;
using Infrastructure.QuestService.Abstract;
using R3;
using Reflex.Attributes;
using UnityEngine;
using UnityEngine.Localization.SmartFormat.PersistentVariables;

namespace Configuration.QuestTasks.Unique
{
    [Serializable]
    public class TutorialTask : TaskBase, IDisposable
    {
        [SerializeField]
        private int requiredCount;

        [SerializeField]
        private TaskConfig config;

        private IBallInteractionMediator ballInteractionMediator;
        private INetworkStateHolder<ConnectionState> connectionStateHolder;

        private CompositeDisposable disposable;
        private Dictionary<uint, int> holders;
        private Dictionary<uint, int> kickers;
        private BallSharedState previousBallState;

        [Inject]
        private void Construct(
            IBallInteractionMediator ballInteractionMediator,
            INetworkStateHolder<ConnectionState> connectionStateHolder
        )
        {
            this.ballInteractionMediator = ballInteractionMediator;
            this.connectionStateHolder = connectionStateHolder;

            holders = new Dictionary<uint, int>();
            kickers = new Dictionary<uint, int>();
            disposable = new CompositeDisposable();
        }

        public override void Start()
        {
            previousBallState = ballInteractionMediator.BallState.CurrentValue;
            ballInteractionMediator.BallState.Subscribe(OnBallStateChanged).AddTo(disposable);
            connectionStateHolder.Subscribe(_ => UpdateState()).AddTo(disposable);

            UpdateState();
        }

        private void OnBallStateChanged(BallSharedState newState)
        {
            if (newState.LastActionId == previousBallState.LastActionId)
            {
                return;
            }

            if (newState.LastActionType == BallActionType.Hold && newState.HolderNetId != 0)
            {
                holders.TryGetValue(newState.HolderNetId, out int count);
                count++;
                count = Mathf.Clamp(count, 0, requiredCount);
                holders[newState.HolderNetId] = count;
            }
            else if (newState.LastActionType == BallActionType.Kick && newState.LastKickInitiatorNetId != 0)
            {
                kickers.TryGetValue(newState.LastKickInitiatorNetId, out int count);
                count++;
                count = Mathf.Clamp(count, 0, requiredCount);
                kickers[newState.LastKickInitiatorNetId] = count;
            }

            previousBallState = newState;
            UpdateState();
        }

        private void UpdateState()
        {
            HashSet<uint> players = connectionStateHolder.GetState().Players;

            if (players == null || players.Count == 0)
            {
                return;
            }

            IList<object> args = new List<object>
            {
                new IntVariable { Value = holders.Sum(item => item.Value) },
                new IntVariable { Value = kickers.Sum(item => item.Value) },
                new IntVariable { Value = requiredCount * players.Count }
            };
            DisplayData.Value = (config.Title.GetLocalizedString(), config.Description.GetLocalizedString(args));

            IsDone.Value = players.All(playerId =>
                holders.TryGetValue(playerId, out int holdCount) && holdCount >= requiredCount &&
                kickers.TryGetValue(playerId, out int kickerCount) && kickerCount >= requiredCount
            );
        }

        void IDisposable.Dispose()
        {
            disposable?.Dispose();
        }
    }
}
