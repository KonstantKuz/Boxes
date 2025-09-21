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

        void IInstaller.InstallBindings(ContainerBuilder containerBuilder)
        {
            this.Log(LogType.Log, "InstallBindings");
            containerBuilder.AddSingleton(service, service.GetType().GetInterfaces());
            containerBuilder.AddSingleton(mediator, mediator.GetType().GetInterfaces());
        }
    }
}
