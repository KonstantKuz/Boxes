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
            containerBuilder.AddSingleton(networkService, networkService.GetType().GetInterfaces());

            containerBuilder.AddSingleton(
                (CustomNetworkManager) NetworkManager.singleton, typeof(CustomNetworkManager).GetInterfaces()
            );

            TypeByteMapper.RegisterTypes<INetworkState>();
            TypeByteMapper.RegisterTypes<INetworkCommand>();
        }
    }
}
