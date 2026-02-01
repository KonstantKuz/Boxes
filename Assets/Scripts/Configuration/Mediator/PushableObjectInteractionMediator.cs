using System;
using System.Collections.Generic;
using Gameplay.Interactable.Abstract;
using Gameplay.Interactable.PushableObjectInteraction;
using Gameplay.Interactable.PushableObjectInteraction.Abstract;

namespace Configuration.Mediator
{
    [Serializable]
    public class PushableObjectInteractionMediator : InteractionMediatorBase<IPushableObjectInteractionInitiator>, IPushableObjectInteractionMediator
    {
        private PushableObject pushableObject;

        PushableObject IPushableObjectInteractionMediator.PushableObject => pushableObject;
        IPushableObjectInteractionInitiator IPushableObjectInteractionMediator.LocalInitiator => localInitiator;
        IReadOnlyDictionary<uint, IPushableObjectInteractionInitiator> IPushableObjectInteractionMediator.Initiators => initiators;

        void IPushableObjectInteractionMediator.RegisterPushableObject(PushableObject pushableObject)
        {
            this.pushableObject = pushableObject;
        }

        void IPushableObjectInteractionMediator.RegisterInitiator(IPushableObjectInteractionInitiator initiator, bool isLocalPlayer)
        {
            RegisterInitiatorInternal(initiator, initiator.NetId, isLocalPlayer);
        }

        bool IPushableObjectInteractionMediator.IsLocalInitiator(uint netId)
        {
            return IsLocalInitiator(netId);
        }
    }
}
