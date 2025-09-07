using Gameplay.Interactable.BoxesInteraction.Abstract;
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
            containerBuilder.AddSingleton(mediator, mediator.GetType().GetInterfaces());
        }
    }
}
