#if DEBUG
using Infrastructure.Cheats;
using UnityEngine;

namespace Configuration.Mediator
{
    public partial class CameraServiceMediator : ICheatsProvider
    {
        bool ICheatsProvider.IsOpen { get; set; }

        string ICheatsProvider.GetLabel()
        {
            return "CameraServiceMediator";
        }

        void ICheatsProvider.RenderCheats()
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label($"Shared mode enabled = {sharedMode}");
            if (GUILayout.Button($"Switch"))
            {
                sharedMode = !sharedMode;
            }
            GUILayout.EndHorizontal();
        }
    }
}
#endif
