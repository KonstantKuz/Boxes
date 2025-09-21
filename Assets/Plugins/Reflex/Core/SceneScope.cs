using System;
using System.Collections.Generic;
using System.Linq;
using Reflex.Injectors;
using Reflex.Logging;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Reflex.Core
{
    [DefaultExecutionOrder(int.MinValue)]
    public sealed class SceneScope : MonoBehaviour
    {
        public static Action<Scene, ContainerBuilder> OnSceneContainerBuilding;

        private void Awake()
        {
            UnityInjector.OnSceneLoaded.Invoke(gameObject.scene, this);
        }

        public void InstallBindings(ContainerBuilder containerBuilder)
        {
            List<IInstaller> installers = GetComponentsInChildren<IInstaller>(true).ToList();

            for (var i = 0; i < installers.Count; i++)
            {
                installers[i].InstallBindings(containerBuilder);
            }

            ReflexLogger.Log($"SceneScope ({gameObject.scene.name}) Bindings Installed", LogLevel.Info, gameObject);
        }
    }
}
