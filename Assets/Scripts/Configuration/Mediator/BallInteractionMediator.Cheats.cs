using Gameplay.Interactable.BallInteraction.Command;
using Infrastructure.Cheats;
using UnityEngine;

#if DEBUG
namespace Configuration.Mediator
{
    public partial class BallInteractionMediator : ICheatsProvider
    {
        bool ICheatsProvider.IsOpen { get; set; }

        string ICheatsProvider.GetLabel()
        {
            return "Ball Interaction";
        }

        void ICheatsProvider.RenderCheats()
        {
            if (GUILayout.Button("Get ball"))
            {
                networkService.SendCommand(new CaptureCommand(initiator.NetId));
            }
        }
    }
}
#endif
