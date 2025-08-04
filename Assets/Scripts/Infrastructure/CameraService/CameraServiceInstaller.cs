using Reflex.Core;
using UnityEngine;

namespace Infrastructure.CameraService
{
    public class CameraServiceInstaller : MonoBehaviour, IInstaller
    {
        [SerializeReference]
        private ICameraService service;

        [SerializeReference]
        private ICameraServiceMediator mediator;

        public void InstallBindings(ContainerBuilder containerBuilder)
        {
            containerBuilder.AddSingleton(service, service.GetType().GetInterfaces());
            containerBuilder.AddSingleton(mediator, mediator.GetType().GetInterfaces());
        }
    }
}
