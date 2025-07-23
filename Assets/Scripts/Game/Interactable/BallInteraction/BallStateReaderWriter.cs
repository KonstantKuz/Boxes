using Mirror;

// ReSharper disable UnusedType.Global
// ReSharper disable UnusedMember.Global
namespace Game.Interactable.BallInteraction
{
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
