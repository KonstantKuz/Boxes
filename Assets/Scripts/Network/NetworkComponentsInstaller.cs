using System;
using Mirror;
using Reflex.Core;

namespace Network
{
    [Serializable]
    public class NetworkComponentsInstaller : IInstaller
    {
        public void InstallBindings(ContainerBuilder containerBuilder)
        {
            containerBuilder.AddSingleton(NetworkManager.singleton);
            containerBuilder.AddSingleton(typeof(NetworkService), typeof(NetworkService).GetInterfaces());
        }
    }
}
