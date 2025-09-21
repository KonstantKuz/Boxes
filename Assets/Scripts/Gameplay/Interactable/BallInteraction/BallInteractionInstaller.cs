using Gameplay.Interactable.BallInteraction.Abstract;
using Infrastructure;
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
            this.Log(LogType.Log, "InstallBindings");
            containerBuilder.AddSingleton(mediator, mediator.GetType().GetInterfaces());
        }
    }
}
