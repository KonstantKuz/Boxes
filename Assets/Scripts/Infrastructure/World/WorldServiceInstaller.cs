using Reflex.Core;
using UnityEngine;

namespace Infrastructure.World
{
    public class WorldServiceInstaller : MonoBehaviour, IInstaller
    {
        [SerializeReference]
        private IWorldService worldService;

        void IInstaller.InstallBindings(ContainerBuilder containerBuilder)
        {
            containerBuilder.AddSingleton(worldService, worldService.GetType().GetInterfaces());
        }
    }
}
