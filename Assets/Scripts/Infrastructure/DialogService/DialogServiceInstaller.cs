using Infrastructure.DialogService.Abstract;
using Reflex.Core;
using UnityEngine;

namespace Infrastructure.DialogService
{
    public class DialogServiceInstaller : MonoBehaviour, IInstaller
    {
        [SerializeReference]
        private IDialogService service;

        [SerializeReference]
        private IDialogServiceMediator mediator;

        void IInstaller.InstallBindings(ContainerBuilder containerBuilder)
        {
            containerBuilder.AddSingleton(service, service.GetType().GetInterfaces());
            containerBuilder.AddSingleton(mediator, mediator.GetType().GetInterfaces());
        }
    }
}
