using Gameplay.Interactable.PipeInteraction.Abstract;
using Infrastructure;
using Reflex.Core;
using UnityEngine;

namespace Gameplay.Interactable.PipeInteraction
{
    public class PipeInteractionInstaller : MonoBehaviour, IInstaller
    {
        [SerializeReference]
        private IPipeInteractionMediator mediator;

        void IInstaller.InstallBindings(ContainerBuilder containerBuilder)
        {
            this.Log(LogType.Log, "InstallBindings");
            containerBuilder.AddSingleton(mediator, mediator.GetType().GetInterfaces());
        }
    }
}
