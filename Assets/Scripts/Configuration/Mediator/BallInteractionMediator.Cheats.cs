#if DEBUG
using Gameplay.Interactable.BallInteraction;
using Gameplay.Interactable.BallInteraction.Command;
using Infrastructure.Cheats;
using UnityEngine;

namespace Configuration.Mediator
{
    public partial class BallInteractionMediator : ICheatsProvider
    {
        private ConfigRenderer<BallInteractionConfig> ballConfigCheats;

        private ConfigRenderer<BallInteractionConfig> ConfigCheats =>
            ballConfigCheats ??= new ConfigRenderer<BallInteractionConfig>(ballInteractionConfig);

        bool ICheatsProvider.IsOpen { get; set; }

        string ICheatsProvider.GetLabel()
        {
            return "Ball Interaction";
        }

        void ICheatsProvider.RenderCheats()
        {
            if (GUILayout.Button("Get ball"))
            {
                ball.gameObject.SetActive(true);
                networkService.SendCommand(new HoldCommand(localInitiator.NetId));
            }

            if (GUILayout.Button("Show config"))
            {
                ConfigCheats.IsOpen = !ConfigCheats.IsOpen;
            }

            if (ConfigCheats.IsOpen)
            {
                GUILayout.BeginVertical("box");
                ConfigCheats.RenderCheats();
                GUILayout.EndVertical();
                GUILayout.Space(10);
            }
        }
    }
}
#endif
