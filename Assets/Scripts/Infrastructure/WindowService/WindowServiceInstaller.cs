using Reflex.Core;
using UnityEngine;

namespace Infrastructure.WindowService
{
    public class WindowServiceInstaller : MonoBehaviour, IInstaller
    {
        void IInstaller.InstallBindings(ContainerBuilder containerBuilder)
        {
            containerBuilder.AddSingleton(typeof(WindowService), typeof(WindowService).GetInterfaces());
        }
    }
}
