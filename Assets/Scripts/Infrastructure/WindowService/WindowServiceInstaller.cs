using Infrastructure.WindowService.Abstract;
using Reflex.Core;
using UnityEngine;

namespace Infrastructure.WindowService
{
    public class WindowServiceInstaller : MonoBehaviour, IInstaller
    {
        [SerializeReference]
        private IWindowService service;

        [SerializeReference]
        private IWindowServiceMediator mediator;

        void IInstaller.InstallBindings(ContainerBuilder containerBuilder)
        {
            containerBuilder.AddSingleton(service, service.GetType().GetInterfaces());
            containerBuilder.AddSingleton(mediator, mediator.GetType().GetInterfaces());
        }
    }
}
