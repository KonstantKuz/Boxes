using System;
using Gameplay.Interactable.BallInteraction.Abstract;
using Gameplay.Interactable.BallInteraction.Command;
using Infrastructure;
using Infrastructure.Network.Abstract;
using Infrastructure.QuestService.Abstract;
using Infrastructure.World;
using Reflex.Attributes;
using UnityEngine;

namespace Configuration.QuestTasks.Beginning
{
    [Serializable]
    public class PetruhaAppearTask : TaskBase
    {
        [SerializeField]
        private string npcId;

        private IWorldService worldService;
        private INetworkService networkService;

        [Inject]
        private void Construct(IWorldService worldService, INetworkService networkService)
        {
            this.worldService = worldService;
            this.networkService = networkService;
        }

        public override void Start()
        {
            if (!worldService.TryGetById(npcId, out IWorldObject worldObject) ||
                !worldObject.TryGetComponent(out IBallInteractionInitiator initiator))
            {
                this.Log(LogType.Error, "Petruha leave task failed");
                return;
            }

            worldObject.Value.SetActive(true);

            networkService.SendCommand(new HoldCommand(initiator.NetId));
            IsDone.Value = true;
        }
    }
}
