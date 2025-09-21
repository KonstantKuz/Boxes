using Infrastructure;
using Reflex.Core;
using UnityEngine;

namespace UI.Dialog
{
    public class DialogWindowInstaller : MonoBehaviour, IInstaller
    {
        [SerializeField]
        private DialogWindow dialogWindowPrefab;

        void IInstaller.InstallBindings(ContainerBuilder containerBuilder)
        {
            this.Log(LogType.Log, "InstallBindings");
            DialogWindow dialogWindow = Instantiate(dialogWindowPrefab);
            containerBuilder.AddSingleton(dialogWindow, dialogWindow.GetType().GetInterfaces());
        }
    }
}
