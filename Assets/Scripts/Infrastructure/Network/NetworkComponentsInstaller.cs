using Mirror;
using Reflex.Core;
using UnityEngine;

namespace Infrastructure.Network
{
    public class NetworkComponentsInstaller : MonoBehaviour, IInstaller
    {
        void IInstaller.InstallBindings(ContainerBuilder containerBuilder)
        {
            containerBuilder.AddSingleton(NetworkManager.singleton);
        }
    }
}
