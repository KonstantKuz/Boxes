using Game.Interactable.BallInteraction;
using Game.Interactable.Dialog;
using Infrastructure;
using Infrastructure.InteractionService;
using Reflex.Core;

namespace Game.Interactable.Installer
{
    // ReSharper disable once UnusedType.Global
    public class InteractionTypesInstaller : IInstaller
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
