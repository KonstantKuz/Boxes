using System;
using Infrastructure;
using Infrastructure.NavigationService;
using Infrastructure.QuestService.Abstract;
using Infrastructure.World;
using Reflex.Attributes;
using UnityEngine;

namespace Configuration.QuestTasks.Shared
{
    [Serializable]
    public class ShowMarkerTask : TaskBase, IDisposable
    {
        [WorldObjectId]
        [SerializeField]
        private string targetId;

        private IWorldService worldService;
        private INavigationService navigationService;

        [Inject]
        private void Construct(IWorldService worldService, INavigationService navigationService)
        {
            this.worldService = worldService;
            this.navigationService = navigationService;
        }

        public override void Start()
        {
            if (!worldService.TryGetById(targetId, out IWorldObject target))
            {
                this.Log(LogType.Error, "Target id not found.");
                return;
            }

            navigationService.SetActiveTarget(target.Value.transform);
            IsDone.Value = true;
        }

        void IDisposable.Dispose()
        {
            navigationService.SetActiveTarget(null);
        }
    }
}
