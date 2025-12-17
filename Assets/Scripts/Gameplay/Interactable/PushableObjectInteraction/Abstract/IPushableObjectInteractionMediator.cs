using System.Collections.Generic;
using Infrastructure.Bootstrap;

namespace Gameplay.Interactable.PushableObjectInteraction.Abstract
{
    public interface IPushableObjectInteractionMediator : IPostBuildInjectable
    {
        PushableObject PushableObject { get; }
        IReadOnlyDictionary<uint, IPushableObjectInteractionInitiator> Initiators { get; }
        void RegisterPushableObject(PushableObject pushableObject);
        void RegisterInitiator(IPushableObjectInteractionInitiator initiator, bool isLocalPlayer);
    }
}