using System.Collections.Generic;

namespace Gameplay.Interactable.Abstract
{
    public abstract class InteractionMediatorBase<TInitiator> where TInitiator : class
    {
        protected TInitiator localInitiator;
        protected List<TInitiator> localInitiators;
        protected Dictionary<uint, TInitiator> initiators;

        protected InteractionMediatorBase()
        {
            localInitiators = new List<TInitiator>();
            initiators = new Dictionary<uint, TInitiator>();
        }

        protected void RegisterInitiatorInternal(TInitiator initiator, uint netId, bool isLocal)
        {
            if (isLocal)
            {
                localInitiator = initiator;
                localInitiators.Add(initiator);
            }

            initiators.Add(netId, initiator);
        }

        protected bool IsLocalInitiator(uint netId)
        {
            return initiators.TryGetValue(netId, out TInitiator initiator) && localInitiators.Contains(initiator);
        }
    }
}
