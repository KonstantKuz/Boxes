using System.Collections.Generic;
using Gameplay.Interactable.PipeInteraction.State;
using Infrastructure.Bootstrap;
using R3;
using UnityEngine;

namespace Gameplay.Interactable.PipeInteraction.Abstract
{
    public interface IPipeInteractionMediator : IPostBuildInjectable
    {
        ReadOnlyReactiveProperty<PipeSharedState> PipeState { get; }
        IReadOnlyDictionary<uint, IPipeInteractionInitiator> Initiators { get; }
        void RegisterPipe(Pipe pipe);
        void RegisterInitiator(IPipeInteractionInitiator initiator, bool isLocalPlayer);
        (Vector3 faceDirection, Vector3 position) GetTransformState(uint playerNetId);
    }
}
