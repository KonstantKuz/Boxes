using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using Gameplay.Interactable.Door;
using Infrastructure;
using Infrastructure.Network.Abstract;
using Infrastructure.QuestService.Abstract;
using Infrastructure.World;
using R3;
using Reflex.Attributes;
using UnityEngine;

namespace Configuration.QuestTasks.Unique
{
    [Serializable]
    public class DoorPatrolTask : TaskBase, IDisposable
    {
        [WorldObjectId]
        [SerializeField]
        private List<string> doorIds;

        [SerializeField]
        private float disturbPeriod = 10f;

        [SerializeField]
        private float knockInterval = 2f;

        private IWorldService worldService;
        private INetworkManager networkManager;
        private List<Door> doors;
        private CompositeDisposable disposables;
        private System.Random random;

        [Inject]
        private void Construct(IWorldService worldService, INetworkManager networkManager)
        {
            this.worldService = worldService;
            this.networkManager = networkManager;

            doors = new List<Door>();
            disposables = new CompositeDisposable();
            random = new System.Random();
        }

        public override void Start()
        {
            IsDone.Value = true;

            if (!networkManager.IsServer)
            {
                return;
            }

            foreach (string doorId in doorIds)
            {
                if (worldService.TryGetById(doorId, out IWorldObject worldObject))
                {
                    if (worldObject.TryGetComponent(out Door door))
                    {
                        doors.Add(door);
                    }
                    else
                    {
                        this.Log(LogType.Warning, $"WorldObject with id {doorId} does not have Door component");
                    }
                }
                else
                {
                    this.Log(LogType.Warning, $"Door with id {doorId} not found");
                }
            }

            if (doors.Count == 0)
            {
                this.Log(LogType.Error, "No doors found for DoorPatrolTask");
                IsDone.Value = true;
                return;
            }

            StartDisturbingDoors();
        }

        private void StartDisturbingDoors()
        {
            if (doors == null || doors.Count == 0)
            {
                return;
            }

            CancellationDisposable cancellationDisposable = new CancellationDisposable();
            disposables.Add(cancellationDisposable);
            UniTask.Void(async (cancellationToken) =>
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    await PatrolSequence(cancellationToken);
                }
            },
            cancellationDisposable.Token);
        }

        private async UniTask PatrolSequence(CancellationToken token)
        {
            await UniTask.WaitForSeconds(disturbPeriod, cancellationToken: token);

            List<Door> availableDoors = doors.Where(door => !door.IsBlocked).ToList();
            int randomIndex = random.Next(availableDoors.Count);
            Door selectedDoor = availableDoors.Count > 0 ? availableDoors[randomIndex] : doors[randomIndex];
            int knockCount = selectedDoor.KnocksBeforeOpen;

            for (int i = 0; i <= knockCount; i++)
            {
                selectedDoor.KnockRpc();
                await UniTask.Delay(TimeSpan.FromSeconds(knockInterval), cancellationToken: token);
            }

            await UniTask.WaitWhile(() => selectedDoor.IsPatrolActive, cancellationToken: token);
        }

        void IDisposable.Dispose()
        {
            disposables?.Dispose();
            doors?.Clear();
        }
    }
}
