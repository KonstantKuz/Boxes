using Mirror;

// ReSharper disable UnusedType.Global
// ReSharper disable UnusedMember.Global
namespace Gameplay.Interactable.BallInteraction
{
    public static class BallStateReaderWriter
    {
        public static void WriteState(this NetworkWriter writer, BallState state)
        {
            writer.WriteUInt(state.OwnerNetId);
        }

        public static BallState ReadState(this NetworkReader reader)
        {
            return new BallState(reader.ReadUInt());
        }
    }
}
