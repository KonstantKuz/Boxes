using Mirror;

namespace Infrastructure.DialogService
{
    // ReSharper disable UnusedType.Global
    // ReSharper disable UnusedMember.Global
    public static class DialogStateReaderWriter
    {
        public static void WriteBallState(this NetworkWriter writer, DialogState state)
        {
            writer.WriteGuid(state.DialogId);
            writer.WriteUInt(state.CurrentIndex);
        }

        public static DialogState ReadBallState(this NetworkReader reader)
        {
            return new DialogState(reader.ReadGuid(), reader.ReadUInt());
        }
    }
}
