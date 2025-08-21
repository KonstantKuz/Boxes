using Reflex.Core;
using UnityEngine;

namespace Infrastructure.NavigationService
{
    public class NavigationServiceInstaller : MonoBehaviour, IInstaller
    {
        [SerializeReference]
        private INavigationService navigationService;

        void IInstaller.InstallBindings(ContainerBuilder containerBuilder)
        {
            containerBuilder.AddSingleton(navigationService, navigationService.GetType().GetInterfaces());
        }
    }
}
