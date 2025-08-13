using System;
using System.Linq;
using Infrastructure.Bootstrap;
using Infrastructure.CanvasRootService;
using Reflex.Attributes;
using Sirenix.Utilities;
using UnityEngine;

namespace UI.HUD
{
    public class HUD : MonoBehaviour, IInitializable, IPostBuildInjectable
    {
        private ICanvasRootService canvasRootService;

        [Inject]
        private void Construct(ICanvasRootService canvasRootService)
        {
            this.canvasRootService = canvasRootService;
        }

        void IInitializable.Initialize()
        {
            canvasRootService.Attach(gameObject);
            GetComponentsInChildren<IInitializable>().Except(new[] { this }).ForEach(item => item.Initialize());
        }

        private void OnDisable()
        {
            GetComponentsInChildren<IDisposable>().ForEach(item => item.Dispose());
        }
    }
}
