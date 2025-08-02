using System;
using Infrastructure.Abstract;
using Infrastructure.CanvasRootService;
using Infrastructure.WindowService.Abstract;
using Reflex.Attributes;
using UnityEngine;

namespace Configuration
{
    [Serializable]
    public class WindowServiceMediator : IWindowServiceMediator
    {
        private ICanvasRootService canvasRootService;

        Type IServiceMediator.BindType => typeof(IWindowServiceMediator);

        [Inject]
        private void Construct(ICanvasRootService canvasRootService)
        {
            this.canvasRootService = canvasRootService;
        }

        void IWindowServiceMediator.AttachToCanvasRoot(IWindow window)
        {
            if (window is not MonoBehaviour windowComponent)
            {
                Debug.LogError("Window must be a MonoBehaviour: " + window.GetType().Name);
                return;
            }

            canvasRootService.Attach(windowComponent.gameObject);
        }
    }
}
