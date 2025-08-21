using System;
using System.Collections.Generic;
using Configuration.QuestTasks.Shared;
using Infrastructure.QuestService.Abstract;
using R3;
using Reflex.Attributes;
using Reflex.Core;
using Reflex.Injectors;
using UnityEngine;

namespace Configuration.QuestTasks.Beginning
{
    [Serializable]
    public class BeginningTaskSequence : ITask
    {
        [SerializeField]
        private NavigationTask goToNpcTask;

        [SerializeField]
        private NpcDialogTask npcDialogTask;

        [SerializeField]
        private NavigationTask goToPlayTask;

        [SerializeField]
        private PassBallTask passBallTask;

        private TaskSequence sequence;

        Observable<(string Title, string Description)> ITask.DisplayData => ((ITask)sequence).DisplayData;
        ReadOnlyReactiveProperty<bool> ITask.IsDone => ((ITask)sequence).IsDone;

        [Inject]
        private void Construct(Container container)
        {
            AttributeInjector.Inject(goToNpcTask, container);
            AttributeInjector.Inject(npcDialogTask, container);
            AttributeInjector.Inject(goToPlayTask, container);
            AttributeInjector.Inject(passBallTask, container);
        }

        void ITask.Start()
        {
            sequence = new TaskSequence();
            sequence.Build(new List<ITask>()
            {
                goToNpcTask, npcDialogTask, goToPlayTask, passBallTask
            });

            sequence?.Start();
        }
    }
}
