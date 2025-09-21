using Infrastructure;
using Reflex.Core;
using UnityEngine;

namespace UI.HUD
{
    public class HUDInstaller : MonoBehaviour, IInstaller
    {
        [SerializeField]
        private HUD hudPrefab;

        void IInstaller.InstallBindings(ContainerBuilder containerBuilder)
        {
            this.Log(LogType.Log, "InstallBindings");
            HUD hud = Instantiate(hudPrefab);
            containerBuilder.AddSingleton(hud, hud.GetType().GetInterfaces());
        }
    }
}
