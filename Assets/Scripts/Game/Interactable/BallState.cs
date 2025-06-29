using System;
using Infrastructure.InteractionService;
using Mirror;
// ReSharper disable UnusedType.Global
// ReSharper disable UnusedMember.Global

namespace Game.Interactable
{
    [Serializable]
    public class BallState : InteractableState
    {
        public BallState()
        {

        }

        public BallState(uint ownerNetId)
        {
            OwnerNetId = ownerNetId;
        }

        public uint OwnerNetId { get; }
    }

    public static class BallStateReaderWriter
    {
        public static void WriteBallState(this NetworkWriter writer, BallState state)
        {
            writer.WriteUInt(state.OwnerNetId);
        }

        public static BallState ReadBallState(this NetworkReader reader)
        {
            return new BallState(reader.ReadUInt());
        }
    }
}
