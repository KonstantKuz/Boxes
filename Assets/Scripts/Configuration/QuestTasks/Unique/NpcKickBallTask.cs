using System;
using Gameplay.Interactable.BallInteraction.Abstract;
using Gameplay.Interactable.BallInteraction.Command;
using Infrastructure;
using Infrastructure.Network.Abstract;
using Infrastructure.QuestService.Abstract;
using Infrastructure.World;
using Reflex.Attributes;
using UnityEngine;

namespace Configuration.QuestTasks.Unique
{
    [Serializable]
    public class NpcKickBallTask : TaskBase, IDisposable
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
            if (!worldService.TryGetById(npcId, out IWorldObject worldObject) ||
                !worldObject.TryGetComponent(out IBallInteractionInitiator initiator))
            {
                this.Log(LogType.Error, $"Could not find npc or invalid npc with id {npcId}");
                return;
            }

            disposable = networkService.ObserveToReact<KickCommand>(_ => IsDone.Value = true);
            networkService.SendCommand(new KickCommand(initiator.NetId, initiator.KickDirection));
        }

        void IDisposable.Dispose()
        {
            disposable?.Dispose();
        }
    }
}
