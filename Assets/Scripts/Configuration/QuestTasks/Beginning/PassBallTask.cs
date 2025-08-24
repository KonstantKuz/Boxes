using System;
using System.Collections.Generic;
using System.Linq;
using Gameplay.Interactable.BallInteraction.Command;
using Infrastructure.Network.Abstract;
using Infrastructure.Network.State;
using Infrastructure.QuestService;
using Infrastructure.QuestService.Abstract;
using R3;
using Reflex.Attributes;
using UnityEngine;
using UnityEngine.Localization.SmartFormat.PersistentVariables;

namespace Configuration.QuestTasks.Beginning
{
    [Serializable]
    public class PassBallTask : TaskBase, IDisposable
    {
        [SerializeField]
        private int requiredCount;

        [SerializeField]
        private TaskConfig config;

        private INetworkService networkService;
        private INetworkStateHolder<ConnectionState> connectionStateHolder;

        private CompositeDisposable disposable;
        private Dictionary<uint, int> holders;
        private Dictionary<uint, int> kickers;

        [Inject]
        private void Construct(
            INetworkService networkService,
            INetworkStateHolder<ConnectionState> connectionStateHolder
        )
        {
            this.networkService = networkService;
            this.connectionStateHolder = connectionStateHolder;

            holders = new Dictionary<uint, int>();
            kickers = new Dictionary<uint, int>();
            disposable = new CompositeDisposable();
        }

        public override void Start()
        {
            networkService.ObserveToExecute<KickCommand>(OnKickExecuted).AddTo(disposable);
            networkService.ObserveToExecute<HoldCommand>(OnHoldExecuted).AddTo(disposable);
            connectionStateHolder.Subscribe(_ => UpdateState()).AddTo(disposable);

            UpdateState();
        }

        private void OnHoldExecuted(HoldCommand holdCommand)
        {
            holders.TryGetValue(holdCommand.InitiatorNetId, out int count);
            count++;
            holders[holdCommand.InitiatorNetId] = count;

            UpdateState();
        }

        private void OnKickExecuted(KickCommand kickCommand)
        {
            kickers.TryGetValue(kickCommand.InitiatorNetId, out int count);
            count++;
            kickers[kickCommand.InitiatorNetId] = count;

            UpdateState();
        }

        private void UpdateState()
        {
            HashSet<uint> players = connectionStateHolder.GetState().Players;

            if (players == null || players.Count == 0)
            {
                return;
            }

            int totalPlayersCount = players.Count;
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
