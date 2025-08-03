using System;
using Infrastructure.CanvasRootService;
using Infrastructure.WindowService.Abstract;
using Reflex.Attributes;
using UnityEngine;

namespace Configuration.Mediator
{
    [Serializable]
    public class WindowServiceMediator : IWindowServiceMediator
    {
        private ICanvasRootService canvasRootService;

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
