using System;
using System.Collections.Generic;
using Gameplay.Interactable.PushableObjectInteraction;
using Gameplay.Interactable.PushableObjectInteraction.Abstract;

namespace Configuration.Mediator
{
    [Serializable]
    public class PushableObjectInteractionMediator : IPushableObjectInteractionMediator
    {
        private Dictionary<uint, IPushableObjectInteractionInitiator> initiators = new();
        private PushableObject pushableObject;

        PushableObject IPushableObjectInteractionMediator.PushableObject => pushableObject;
        IReadOnlyDictionary<uint, IPushableObjectInteractionInitiator> IPushableObjectInteractionMediator.Initiators => initiators;

        void IPushableObjectInteractionMediator.RegisterPushableObject(PushableObject pushableObject)
        {
            this.pushableObject = pushableObject;
        }

        void IPushableObjectInteractionMediator.RegisterInitiator(IPushableObjectInteractionInitiator initiator, bool isLocalPlayer)
        {
            initiators.Add(initiator.NetId, initiator);
        }
    }
}
