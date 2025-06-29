using System.Collections.Generic;
using Reflex.Core;
using UnityEngine;

namespace Game
{
    public class Bootstrap : MonoBehaviour, IInstaller
    {
        [SerializeReference]
        private List<IInstaller> installers;

        public void InstallBindings(ContainerBuilder containerBuilder)
        {
            foreach (IInstaller installer in installers)
            {
                installer.InstallBindings(containerBuilder);
            }
        }
    }
}
