using System;
using Gameplay.Interactable.BallInteraction.Abstract;
using Gameplay.Interactable.BallInteraction.Command;
using Infrastructure;
using Infrastructure.Network.Abstract;
using Infrastructure.Network.Components;
using Infrastructure.QuestService.Abstract;
using Infrastructure.World;
using Reflex.Attributes;
using UnityEngine;

namespace Configuration.QuestTasks.Unique
{
    [Serializable]
    public class NpcHoldBallTask : TaskBase
    {
        [SerializeField]
        private string npcId;

        private IWorldService worldService;
        private INetworkService networkService;
        private INetworkManager networkManager;
        private IBallInteractionMediator ballInteractionMediator;

        [Inject]
        private void Construct(
            IWorldService worldService,
            INetworkService networkService,
            INetworkManager networkManager,
            IBallInteractionMediator ballInteractionMediator
        )
        {
            this.worldService = worldService;
            this.networkService = networkService;
            this.networkManager = networkManager;
            this.ballInteractionMediator = ballInteractionMediator;
        }

        public override void Start()
        {
            if (!worldService.TryGetById(npcId, out IWorldObject worldObject) ||
                !worldObject.TryGetComponent(out IBallInteractionInitiator initiator))
            {
                this.Log(LogType.Error, $"Could not find npc or invalid npc with id {npcId}");
                return;
            }

            if (networkManager.IsServer && ballInteractionMediator.Ball != null &&
                ballInteractionMediator.Ball.TryGetComponent(out NetworkStateHelper ball))
            {
                ball.CmdSetActive(true);
                networkService.SendCommand(new HoldCommand(initiator.NetId));
            }

            IsDone.Value = true;
        }
    }
}
