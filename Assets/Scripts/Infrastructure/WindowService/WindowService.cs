using System.Collections.Generic;
using UnityEngine;

namespace Infrastructure.WindowService
{
    // ReSharper disable once UnusedType.Global
    public class WindowService : IWindowService
    {
        private readonly Dictionary<string, IWindow> windows = new();

        public IWindow ActiveWindow { get; private set; }

        void IWindowService.RegisterWindow(string id, IWindow window)
        {
            if (!windows.TryAdd(id, window))
            {
                Debug.LogWarning("Window with this ID already exists: " + id);
            }
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
                ActiveWindow.Show();
            }
            else
            {
                Debug.LogWarning("Window not found: " + id);
            }
        }

        public void HideActiveWindow()
        {
            ActiveWindow?.Hide();
            ActiveWindow = null;
        }
    }
}
