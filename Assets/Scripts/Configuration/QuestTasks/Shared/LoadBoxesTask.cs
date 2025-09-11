using System;
using System.Collections.Generic;
using Gameplay.Interactable.BoxesInteraction.Components;
using Infrastructure;
using Infrastructure.QuestService;
using Infrastructure.QuestService.Abstract;
using Infrastructure.World;
using ObservableCollections;
using R3;
using Reflex.Attributes;
using UnityEngine;
using UnityEngine.Localization.SmartFormat.PersistentVariables;

namespace Configuration.QuestTasks.Shared
{
    public class LoadBoxesTask : TaskBase, IDisposable
    {
        [SerializeField]
        private TaskConfig config;

        [SerializeField]
        private string sourceStorageId;

        [SerializeField]
        private string targetStorageId;

        private IWorldService worldService;

        private int requiredCount;
        private BoxesStorage sourceStorage;
        private BoxesStorage targetStorage;
        private IDisposable disposable;

        [Inject]
        private void Construct(IWorldService worldService)
        {
            this.worldService = worldService;
        }

        public override void Start()
        {
            if (!worldService.TryGetById(Guid.Parse(sourceStorageId), out IWorldObject source) ||
                !source.Value.TryGetComponent(out sourceStorage))
            {
                this.Log(LogType.Error, $"Could not find storage with id {sourceStorageId}");
                return;
            }

            if (!worldService.TryGetById(Guid.Parse(targetStorageId), out IWorldObject target) ||
                !target.Value.TryGetComponent(out targetStorage))
            {
                this.Log(LogType.Error, $"Could not find storage with id {targetStorageId}");
                return;
            }

            requiredCount = sourceStorage.Boxes.Count;
            disposable = targetStorage.Boxes.ObserveCountChanged(true).Subscribe(_ => UpdateState());
        }

        private void UpdateState()
        {
            IList<object> args = new List<object>
            {
                new IntVariable { Value = targetStorage.Boxes.Count },
                new IntVariable { Value = requiredCount }
            };

            DisplayData.Value = (config.Title.GetLocalizedString(), config.Description.GetLocalizedString(args));
            IsDone.Value = sourceStorage.Boxes.Count <= 0;
        }

        void IDisposable.Dispose()
        {
            disposable?.Dispose();
        }
    }
}
