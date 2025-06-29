using Game.Interactable;
using Infrastructure;
using Infrastructure.InteractionService;
using Reflex.Core;

namespace Game
{
    // ReSharper disable once UnusedType.Global
    public class TypeMappersInstaller : IInstaller
    {
        void IInstaller.InstallBindings(ContainerBuilder containerBuilder)
        {
            TypeByteMapper<InteractionContext>.RegisterTypes();
        }
    }
}
