using Mirror;

namespace Infrastructure.DialogService
{
    // ReSharper disable UnusedType.Global
    // ReSharper disable UnusedMember.Global
    public static class DialogStateReaderWriter
    {
        public static void WriteState(this NetworkWriter writer, DialogState state)
        {
            writer.WriteGuid(state.DialogId);
            writer.WriteByte(state.ReplicaIndex);
            writer.WriteHashSet(state.ReadyPlayers);
        }

        public static DialogState ReadState(this NetworkReader reader)
        {
            return new DialogState(reader.ReadGuid(), reader.ReadByte(), reader.ReadHashSet<uint>());
        }
    }
}
