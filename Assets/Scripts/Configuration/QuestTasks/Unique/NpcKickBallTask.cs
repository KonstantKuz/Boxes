using System;
using Gameplay.Interactable.BallInteraction.Abstract;
using Gameplay.Interactable.BallInteraction.State;
using Infrastructure;
using Infrastructure.QuestService.Abstract;
using Infrastructure.World;
using Reflex.Attributes;
using UnityEngine;

namespace Configuration.QuestTasks.Unique
{
    [Serializable]
    public class NpcKickBallTask : TaskBase
    {
        [WorldObjectId]
        [SerializeField]
        private string npcId;

        private IWorldService worldService;
        private IBallInteractionMediator ballInteractionMediator;

        [Inject]
        private void Construct(IWorldService worldService, IBallInteractionMediator ballInteractionMediator)
        {
            this.worldService = worldService;
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

            if (ballInteractionMediator.Ball != null)
            {
                BallSharedState current = ballInteractionMediator.Ball.StateHolder.GetState();
                ballInteractionMediator.Ball.StateHolder.WriteState(new BallSharedState(
                    kicksCount: 0,
                    ownerNetId: initiator.NetId,
                    holderNetId: 0,
                    lastActionId: current.LastActionId + 1,
                    lastActionType: BallActionType.Kick,
                    lastKickDirection: initiator.KickDirection,
                    lastKickInitiatorNetId: initiator.NetId
                ));
            }

            IsDone.Value = true;
        }
    }
}
