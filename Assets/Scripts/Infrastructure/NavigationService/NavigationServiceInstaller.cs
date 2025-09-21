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
            this.Log(LogType.Log, "InstallBindings");
            containerBuilder.AddSingleton(navigationService, navigationService.GetType().GetInterfaces());
        }
    }
}
