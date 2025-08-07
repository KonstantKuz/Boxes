using Infrastructure.Network.Abstract;
using Infrastructure.Network.State;
using Mirror;
using Reflex.Core;
using UnityEngine;

namespace Infrastructure.Network
{
    public class NetworkComponentsInstaller : MonoBehaviour, IInstaller
    {
        [SerializeField]
        private NetworkService networkService;

        [SerializeField]
        private ConnectionStateHolder connectionStateHolder;

        [SerializeReference]
        private INetworkSerializer networkSerializer;

        void IInstaller.InstallBindings(ContainerBuilder containerBuilder)
        {
            containerBuilder.AddSingleton(networkService, networkService.GetType().GetInterfaces());
            containerBuilder.AddSingleton(connectionStateHolder, connectionStateHolder.GetType().GetInterfaces());
            containerBuilder.AddSingleton(networkSerializer, networkSerializer.GetType().GetInterfaces());

            containerBuilder.AddSingleton(
                (CustomNetworkManager) NetworkManager.singleton, typeof(CustomNetworkManager).GetInterfaces()
            );

            TypeByteMapper byteMapper = TypeByteMapper.Build(typeof(INetworkState), typeof(INetworkCommand));

            containerBuilder.AddSingleton(byteMapper, byteMapper.GetType().GetInterfaces());
        }
    }
}
