using System;
using Infrastructure;
using Infrastructure.QuestService.Abstract;
using Infrastructure.World;
using Reflex.Attributes;
using UnityEngine;

namespace Configuration.QuestTasks.Shared
{
    [Serializable]
    public class SetActiveTask : TaskBase
    {
        [SerializeField]
        private string targetId;

        [SerializeField]
        private string positionId;

        [SerializeField]
        private bool value = true;

        private IWorldService worldService;

        [Inject]
        private void Construct(IWorldService worldService)
        {
            this.worldService = worldService;
        }

        public override void Start()
        {
            if (!worldService.TryGetById(targetId, out IWorldObject worldObject))
            {
                this.Log(LogType.Error, $"Target object with id {targetId} was not found");
                return;
            }

            IWorldObject positionObject = null;

            if (!string.IsNullOrEmpty(positionId) &&
                !worldService.TryGetById(positionId, out positionObject))
            {
                this.Log(LogType.Error, $"Position object with id {positionId} was not found");
                return;
            }

            worldObject.Value.SetActive(value);
            if (positionObject != null)
            {
                worldObject.Value.transform.position = positionObject.Value.transform.position;
                worldObject.Value.transform.rotation = positionObject.Value.transform.rotation;
            }
            IsDone.Value = true;
        }
    }
}
