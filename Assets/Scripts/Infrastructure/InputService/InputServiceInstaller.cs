using Infrastructure.InputService.Abstract;
using Reflex.Core;
using UnityEngine;

namespace Infrastructure.InputService
{
    public class InputServiceInstaller : MonoBehaviour, IInstaller
    {
        [SerializeReference]
        private IInputService inputService;

        [SerializeReference]
        private IInputServiceMediator mediator;

        void IInstaller.InstallBindings(ContainerBuilder containerBuilder)
        {
            this.Log(LogType.Log, "InstallBindings");
            containerBuilder.AddSingleton(inputService, inputService.GetType().GetInterfaces());
            containerBuilder.AddSingleton(mediator, mediator.GetType().GetInterfaces());
        }
    }
}
