using System.Collections.Generic;
using Reflex.Core;
using UnityEngine;

namespace Infrastructure.DialogService
{
    public class DialogServiceInstaller : IInstaller
    {
        [SerializeField]
        private List<DialogSequence> dialogs;

        public List<DialogSequence> Dialogs => dialogs;

        public void InstallBindings(ContainerBuilder containerBuilder)
        {
            containerBuilder.AddSingleton(this);
            containerBuilder.AddSingleton(typeof(DialogService), typeof(IDialogService));
        }
    }
}
