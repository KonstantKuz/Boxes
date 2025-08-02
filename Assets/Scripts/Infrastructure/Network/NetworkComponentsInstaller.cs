using Mirror;
using Reflex.Core;
using UnityEngine;

namespace Infrastructure.Network
{
    public class NetworkComponentsInstaller : MonoBehaviour, IInstaller
    {
        [SerializeField]
        private NetworkService networkService;

        void IInstaller.InstallBindings(ContainerBuilder containerBuilder)
        {
            containerBuilder.AddSingleton(NetworkManager.singleton);
            containerBuilder.AddSingleton(networkService, networkService.GetType().GetInterfaces());
        }
    }
}
