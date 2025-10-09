using System.Collections.Generic;
using Infrastructure.Network.Abstract;
using MessagePack;
using UnityEngine;

namespace Gameplay.Interactable.PipeInteraction.State
{
    [MessagePackObject]
    public struct PipeSharedState : INetworkState
    {
        public static PipeSharedState Default => new(new Dictionary<uint, Vector2>());

        [Key(0)]
        public Dictionary<uint, Vector2> PlayerInputs { get; }

        [IgnoreMember]
        public int PlayerCount => PlayerInputs?.Count ?? 0;

        public PipeSharedState(Dictionary<uint, Vector2> playerInputs)
        {
            PlayerInputs = playerInputs ?? new Dictionary<uint, Vector2>();
        }

        public bool HasPlayer(uint playerNetId)
        {
            return PlayerInputs.ContainsKey(playerNetId);
        }
    }
}
