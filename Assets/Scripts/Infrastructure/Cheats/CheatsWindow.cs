using System.Collections.Generic;
using System.Linq;
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

        private List<ICheatsProvider> providers;
        private bool isActive;
        private Vector2 scrollPosition;

        private Rect cheatsWindowRect;

        [Inject]
        private void Construct(IEnumerable<ICheatsProvider> providers)
        {
            this.providers = providers.ToList();
        }

        private void Awake()
        {
            cheatsAction.Enable();
            cheatsAction.performed += _ => isActive = !isActive;

            cheatsWindowRect = new Rect(Screen.width / 2f, Screen.height / 2f, windowWidth, windowHeight);
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
