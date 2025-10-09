using System;
using Infrastructure.QuestService.Abstract;
using R3;
using UnityEngine;

namespace Configuration.QuestTasks.Shared
{
    [Serializable]
    public class WaitForSecondsTask : TaskBase, IDisposable
    {
        [SerializeField]
        private float seconds;

        private IDisposable disposable;

        public override void Start()
        {
            disposable = Observable
                .Timer(TimeSpan.FromSeconds(seconds), UnityTimeProvider.Update)
                .Subscribe(_ => IsDone.Value = true);
        }

        void IDisposable.Dispose()
        {
            disposable?.Dispose();
        }
    }
}
