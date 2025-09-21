using Reflex.Core;
using UnityEngine;

namespace Infrastructure.CanvasRootService
{
    public class CanvasRootServiceInstaller : MonoBehaviour, IInstaller
    {
        [SerializeField]
        private CanvasRoot canvasRootPrefab;

        void IInstaller.InstallBindings(ContainerBuilder containerBuilder)
        {
            this.Log(LogType.Log, "InstallBindings");
            CanvasRoot canvasRoot = Instantiate(canvasRootPrefab);
            containerBuilder.AddSingleton(canvasRoot);
            containerBuilder.AddSingleton(typeof(CanvasRootService), typeof(CanvasRootService).GetInterfaces());
        }
    }
}
