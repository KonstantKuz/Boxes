using Reflex.Core;
using UnityEngine;

namespace Infrastructure.InputService
{
    public class InputServiceInstaller : MonoBehaviour, IInstaller
    {
        void IInstaller.InstallBindings(ContainerBuilder containerBuilder)
        {
            containerBuilder.AddSingleton(typeof(InputService), typeof(InputService).GetInterfaces());
        }
    }
}
