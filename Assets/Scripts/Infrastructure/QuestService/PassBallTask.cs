using System;
using System.Collections.Generic;
using System.Linq;
using Gameplay.Interactable.BallInteraction.Command;
using Infrastructure.Network.Abstract;
using Infrastructure.Network.State;
using R3;
using Reflex.Attributes;
using UnityEngine.Localization.SmartFormat.PersistentVariables;

namespace Infrastructure.QuestService
{
    public class PassBallTask : TaskBase, IDisposable
    {
        private INetworkService networkService;
        private INetworkStateHolder<ConnectionState> connectionStateHolder;

        private CompositeDisposable disposable;
        private TaskConfig config;

        private HashSet<uint> holders;
        private HashSet<uint> kickers;

        [Inject]
        private void Construct(
            INetworkService networkService,
            INetworkStateHolder<ConnectionState> connectionStateHolder
        )
        {
            this.networkService = networkService;
            this.connectionStateHolder = connectionStateHolder;

            holders = new HashSet<uint>();
            kickers = new HashSet<uint>();
        }

        public void Initialize(TaskConfig config)
        {
            this.config = config;

            disposable = new CompositeDisposable();
            networkService.ObserveToExecute<KickCommand>(OnKickExecuted).AddTo(disposable);
            networkService.ObserveToExecute<HoldCommand>(OnHoldExecuted).AddTo(disposable);
            connectionStateHolder.Subscribe(_ => UpdateState()).AddTo(disposable);

            UpdateState();
        }

        private void OnHoldExecuted(HoldCommand holdCommand)
        {
            if (holders.Add(holdCommand.InitiatorNetId))
            {
                UpdateState();
            }
        }

        private void OnKickExecuted(KickCommand kickCommand)
        {
            if (kickers.Add(kickCommand.InitiatorNetId))
            {
                UpdateState();
            }
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
                new IntVariable { Value = holders.Count },
                new IntVariable { Value = kickers.Count },
                new IntVariable { Value = totalPlayersCount }
            };
            DisplayData.Value = (config.Title.GetLocalizedString(), config.Description.GetLocalizedString(args));
            IsDone.Value = players.All(id => holders.Contains(id) && kickers.Contains(id));
        }

        void IDisposable.Dispose()
        {
            disposable?.Dispose();
        }
    }
}
