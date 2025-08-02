using System;
using Reflex.Core;
using Reflex.Extensions;
using Reflex.Injectors;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Infrastructure.Bootstrap
{
    public class BootstrapInstaller : MonoBehaviour, IInstaller
    {
        void IInstaller.InstallBindings(ContainerBuilder containerBuilder)
        {
            containerBuilder.OnContainerBuilt += Initialize;
        }

        private void Initialize(Container container)
        {
            foreach (IPostBuildInjectable postBuildInjectable in container.All<IPostBuildInjectable>())
            {
                AttributeInjector.Inject(postBuildInjectable, container);
            }

            foreach (IInitializable initializable in container.All<IInitializable>())
            {
                initializable.Initialize();
            }
        }

        private void OnDestroy()
        {
            foreach (IDisposable disposable in SceneManager.GetActiveScene().GetSceneContainer().All<IDisposable>())
            {
                disposable.Dispose();
            }
        }
    }
}
