using Reflex.Core;
using UnityEngine;

namespace Infrastructure.DialogService
{
    public class DialogServiceInstaller : MonoBehaviour, IInstaller
    {
        [SerializeField]
        private Component dialogService;

        [SerializeField]
        private Component mediator;

        void IInstaller.InstallBindings(ContainerBuilder containerBuilder)
        {
            containerBuilder.AddSingleton(dialogService, dialogService.GetType().GetInterfaces());
            containerBuilder.AddSingleton(mediator, mediator.GetType().GetInterfaces());
        }
    }
}
