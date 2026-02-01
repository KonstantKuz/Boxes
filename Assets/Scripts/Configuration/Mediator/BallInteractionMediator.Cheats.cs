#if DEBUG
using Gameplay.Interactable.BallInteraction;
using Gameplay.Interactable.BallInteraction.Abstract;
using Gameplay.Interactable.BallInteraction.State;
using Infrastructure.Cheats;
using Infrastructure.Network.Components;
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
                if (localInitiators.Count == 0)
                {
                    Debug.LogWarning("No local initiators available");
                    return;
                }

                IBallInteractionInitiator firstLocalInitiator = localInitiators[0];
                ball.GetComponent<NetworkStateHelper>().CmdSetActive(true);

                BallSharedState current = ball.StateHolder.GetState();

                ball.StateHolder.WriteState(new BallSharedState(
                    kicksCount: 0,
                    ownerNetId: firstLocalInitiator.NetId,
                    holderNetId: firstLocalInitiator.NetId,
                    lastActionId: current.LastActionId + 1,
                    lastActionType: BallActionType.Hold,
                    lastKickDirection: current.LastKickDirection,
                    lastKickInitiatorNetId: current.LastKickInitiatorNetId
                ));
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
