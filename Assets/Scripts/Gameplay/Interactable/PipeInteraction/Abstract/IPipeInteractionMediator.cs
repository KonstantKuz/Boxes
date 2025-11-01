using System.Collections.Generic;
using Gameplay.Interactable.PipeInteraction.State;
using Infrastructure.Bootstrap;
using R3;
using UnityEngine;

namespace Gameplay.Interactable.PipeInteraction.Abstract
{
    public interface IPipeInteractionMediator : IPostBuildInjectable
    {
        Pipe Pipe { get; }
        ReadOnlyReactiveProperty<PipeSharedState> PipeState { get; }
        IReadOnlyDictionary<uint, IPipeInteractionInitiator> Initiators { get; }
        void RegisterPipe(Pipe pipe);
        void RegisterInitiator(IPipeInteractionInitiator initiator, bool isLocalPlayer);
    }
}
