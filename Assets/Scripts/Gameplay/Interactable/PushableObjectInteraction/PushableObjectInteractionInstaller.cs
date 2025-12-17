using Gameplay.Interactable.PushableObjectInteraction.Abstract;
using Infrastructure;
using Reflex.Core;
using UnityEngine;

namespace Gameplay.Interactable.PushableObjectInteraction
{
    public class PushableObjectInteractionInstaller : MonoBehaviour, IInstaller
    {
        [SerializeReference]
        private IPushableObjectInteractionMediator mediator;

        void IInstaller.InstallBindings(ContainerBuilder containerBuilder)
        {
            this.Log(LogType.Log, "InstallBindings");
            containerBuilder.AddSingleton(mediator, mediator.GetType().GetInterfaces());
        }
    }
}