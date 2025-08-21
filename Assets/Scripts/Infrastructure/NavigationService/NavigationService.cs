using R3;
using UnityEngine;

namespace Infrastructure.NavigationService
{
    public class NavigationService : INavigationService
    {
        private readonly ReactiveProperty<Transform>  activeTarget = new();
        ReadOnlyReactiveProperty<Transform> INavigationService.ActiveTarget => activeTarget;

        void INavigationService.SetActiveTarget(Transform target)
        {
            activeTarget.Value = target;
        }
    }
}
