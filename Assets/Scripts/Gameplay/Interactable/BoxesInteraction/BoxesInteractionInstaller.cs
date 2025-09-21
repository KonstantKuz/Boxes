using Gameplay.Interactable.BoxesInteraction.Abstract;
using Infrastructure;
using Reflex.Core;
using UnityEngine;

namespace Gameplay.Interactable.BoxesInteraction
{
    public class BoxesInteractionInstaller : MonoBehaviour, IInstaller
    {
        [SerializeReference]
        private IBoxesInteractionMediator mediator;

        void IInstaller.InstallBindings(ContainerBuilder containerBuilder)
        {
            this.Log(LogType.Log, "InstallBindings");
            containerBuilder.AddSingleton(mediator, mediator.GetType().GetInterfaces());
        }
    }
}
