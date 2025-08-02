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
            DialogWindow dialogWindow = Instantiate(dialogWindowPrefab);
            containerBuilder.AddSingleton(dialogWindow, dialogWindow.GetType().GetInterfaces());
        }
    }
}
