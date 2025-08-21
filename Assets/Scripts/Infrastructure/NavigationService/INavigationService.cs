using Infrastructure.Bootstrap;
using R3;
using UnityEngine;

namespace Infrastructure.NavigationService
{
    public interface INavigationService : IPostBuildInjectable
    {
        ReadOnlyReactiveProperty<Transform> ActiveTarget { get; }
        void SetActiveTarget(Transform target);
    }
}
