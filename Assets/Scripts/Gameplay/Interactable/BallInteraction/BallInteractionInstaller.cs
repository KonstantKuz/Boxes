using Gameplay.Interactable.BallInteraction.Abstract;
using Reflex.Core;
using UnityEngine;

namespace Gameplay.Interactable.BallInteraction
{
    public class BallInteractionInstaller : MonoBehaviour, IInstaller
    {
        [SerializeReference]
        private IBallInteractionMediator mediator;

        void IInstaller.InstallBindings(ContainerBuilder containerBuilder)
        {
            containerBuilder.AddSingleton(mediator, mediator.GetType().GetInterfaces());
        }
    }
}
