using System.Collections.Generic;
using System.Linq;
using Infrastructure.InputService.Abstract;
using Reflex.Attributes;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Infrastructure.Cheats
{
    public class CheatsWindow : MonoBehaviour
    {
        [SerializeField]
        private float windowWidth = 300f;

        [SerializeField]
        private float windowHeight = 200f;

        [SerializeField]
        private InputAction cheatsAction;

        private IInputService inputService;
        private List<ICheatsProvider> providers;
        private bool isActive;
        private Vector2 scrollPosition;

        private Rect cheatsWindowRect;

        [Inject]
        private void Construct(IInputService inputService, IEnumerable<ICheatsProvider> providers)
        {
            this.inputService = inputService;
            this.providers = providers.ToList();
        }

        private void Awake()
        {
            cheatsAction.Enable();
            cheatsAction.performed += OnCheatsToggle;

            Vector2 position = new Vector2(Screen.width / 2f - windowWidth / 2, Screen.height / 2f - windowHeight / 2);
            Vector2 size = new Vector2(windowWidth, windowHeight);
            cheatsWindowRect = new Rect(position, size);
        }

        private void OnCheatsToggle(InputAction.CallbackContext context)
        {
            isActive = !isActive;

            if (isActive)
            {
                inputService.DisableAllInputs();
            }
            else
            {
                inputService.EnableAllInputs();
            }
        }

        private void OnGUI()
        {
            if (!isActive)
            {
                return;
            }

            cheatsWindowRect = GUILayout.Window(0, cheatsWindowRect, DrawCheatsWindow, "Cheats");
        }

        private void DrawCheatsWindow(int windowID)
        {
            scrollPosition = GUILayout.BeginScrollView(scrollPosition);

            foreach (ICheatsProvider provider in providers)
            {
                if (GUILayout.Button(provider.GetLabel()))
                {
                    provider.IsOpen = !provider.IsOpen;
                }

                if (provider.IsOpen)
                {
                    GUILayout.BeginVertical("box");
                    provider.RenderCheats();
                    GUILayout.EndVertical();
                    GUILayout.Space(10);
                }
            }

            GUILayout.EndScrollView();

            GUI.DragWindow(new Rect(0, 0, 10000, 10000));
        }
    }
}
