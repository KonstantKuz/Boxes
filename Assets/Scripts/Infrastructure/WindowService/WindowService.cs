using System.Collections.Generic;
using Infrastructure.WindowService.Abstract;
using Reflex.Attributes;
using UnityEngine;

namespace Infrastructure.WindowService
{
    // ReSharper disable once UnusedType.Global
    public class WindowService : IWindowService
    {
        private IWindowServiceMediator mediator;
        private Dictionary<string, IWindow> windows;

        public IWindow ActiveWindow { get; private set; }

        [Inject]
        private void Construct(IWindowServiceMediator windowServiceMediator)
        {
            mediator = windowServiceMediator;
            windows = new Dictionary<string, IWindow>();
        }

        void IWindowService.RegisterWindow(string id, IWindow window)
        {
            if (!windows.TryAdd(id, window))
            {
                Debug.LogWarning("Window with this ID already exists: " + id);
            }

            mediator.AttachToCanvasRoot(window);
            window.Hide();
        }

        void IWindowService.ShowWindow(string id, IWindowContext context)
        {
            if (ActiveWindow?.Id == id)
            {
                Debug.Log("Window is already active: " + id);
                return;
            }

            if (windows.TryGetValue(id, out IWindow window))
            {
                ActiveWindow?.Hide();
                ActiveWindow = window;
                ActiveWindow.Show(context);
            }
            else
            {
                Debug.LogWarning("Window not found: " + id);
            }
        }

        void IWindowService.HideWindow(string id)
        {
            if (ActiveWindow?.Id != id)
            {
                Debug.LogWarning("Active window does not match the ID: " + id);
                return;
            }

            ((IWindowService) this).HideActiveWindow();
        }

        void IWindowService.HideActiveWindow()
        {
            ActiveWindow?.Hide();
            ActiveWindow = null;
        }
    }
}
