using System;
using Gameplay.Interactable.BallInteraction.Abstract;
using Gameplay.Interactable.BallInteraction.State;
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
        [WorldObjectId]
        [SerializeField]
        private string npcId;

        private IWorldService worldService;
        private INetworkManager networkManager;
        private IBallInteractionMediator ballInteractionMediator;

        [Inject]
        private void Construct(
            IWorldService worldService,
            INetworkManager networkManager,
            IBallInteractionMediator ballInteractionMediator
        )
        {
            this.worldService = worldService;
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

            if (ballInteractionMediator.Ball != null &&
                ballInteractionMediator.Ball.TryGetComponent(out NetworkStateHelper ball))
            {
                ball.CmdSetActive(true);

                BallSharedState current = ballInteractionMediator.Ball.StateHolder.GetState();
                ballInteractionMediator.Ball.StateHolder.WriteState(new BallSharedState(
                    kicksCount: current.KicksCount,
                    ownerNetId: initiator.NetId,
                    holderNetId: initiator.NetId,
                    lastActionId: current.LastActionId + 1,
                    lastActionType: BallActionType.Hold,
                    lastKickDirection: Vector3.zero,
                    lastKickInitiatorNetId: current.LastKickInitiatorNetId
                ));
            }

            IsDone.Value = true;
        }
    }
}
