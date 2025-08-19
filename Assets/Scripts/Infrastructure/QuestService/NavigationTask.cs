using R3;
using R3.Triggers;
using UnityEngine;

namespace Infrastructure.QuestService
{
    public class NavigationTask : ITask
    {
        private Observable<bool> isDone;
        private ReactiveProperty<(string, string)> displayData;
        private Transform targetTrigger;
        private TaskConfig config;

        public Vector3 TargetPosition => targetTrigger.position;

        Observable<(string Title, string Description)> ITask.DisplayData => displayData;
        ReadOnlyReactiveProperty<bool> ITask.IsDone => isDone.ToReadOnlyReactiveProperty();

        public void Initialize(TaskConfig config, Collider targetTrigger)
        {
            this.config = config;
            this.targetTrigger = targetTrigger.transform;
            isDone = targetTrigger.OnTriggerEnterAsObservable()
                .Where(item => item.CompareTag("Player"))
                .Select(_ => true);
        }
    }
}