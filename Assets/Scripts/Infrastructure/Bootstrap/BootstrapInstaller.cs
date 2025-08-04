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
        private Container container;

        void IInstaller.InstallBindings(ContainerBuilder containerBuilder)
        {
            containerBuilder.OnContainerBuilt += Initialize;
        }

        private void Initialize(Container container)
        {
            this.container = container;

            foreach (IPostBuildInjectable postBuildInjectable in container.All<IPostBuildInjectable>())
            {
                AttributeInjector.Inject(postBuildInjectable, container);
            }

            foreach (IInitializable initializable in container.All<IInitializable>())
            {
                initializable.Initialize();
            }
        }

        private void Update()
        {
            if (container == null)
            {
                return;
            }

            foreach (IUpdatable updatable in container.All<IUpdatable>())
            {
                updatable.Update();
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
