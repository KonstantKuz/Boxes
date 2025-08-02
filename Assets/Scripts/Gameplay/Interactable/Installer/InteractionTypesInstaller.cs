using Gameplay.Interactable.BallInteraction;
using Gameplay.Interactable.Dialog;
using Infrastructure;
using Infrastructure.InteractionService;
using Infrastructure.InteractionService.Abstract;
using Reflex.Core;
using UnityEngine;

namespace Gameplay.Interactable.Installer
{
    public class InteractionTypesInstaller : MonoBehaviour, IInstaller
    {
        void IInstaller.InstallBindings(ContainerBuilder containerBuilder)
        {
            TypeByteMapper<InteractionContext>.RegisterTypes();

            InteractionTypeExtensions.RegisterReader(typeof(KickInteractionContext), KickInteractionContext.Read);
            InteractionTypeExtensions.RegisterReader(typeof(CaptureInteractionContext), CaptureInteractionContext.Read);
            InteractionTypeExtensions.RegisterReader(typeof(DialogInteractionContext), DialogInteractionContext.Read);
        }
    }
}
