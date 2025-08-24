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
    public class PetruhaLeaveTask : TaskBase, IDisposable
    {
        [SerializeField]
        private string npcId;

        private IWorldService worldService;
        private INetworkService networkService;
        private IDisposable disposable;

        [Inject]
        private void Construct(IWorldService worldService, INetworkService networkService)
        {
            this.worldService = worldService;
            this.networkService = networkService;
        }

        public override void Start()
        {
            if (!worldService.TryGetById(Guid.Parse(npcId), out IWorldObject worldObject) ||
                !worldObject.TryGetComponent(out IBallInteractionInitiator initiator))
            {
                this.Log(LogType.Error, "Petruha leave task failed");
                return;
            }

            disposable = networkService.ObserveToExecute<KickCommand>(_ => IsDone.Value = true);
            networkService.SendCommand(new KickCommand(initiator.NetId, initiator.KickDirection));
        }

        void IDisposable.Dispose()
        {
            disposable?.Dispose();
        }
    }
}
