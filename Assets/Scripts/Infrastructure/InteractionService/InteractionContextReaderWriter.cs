using Infrastructure.InteractionService.Abstract;
using Mirror;

namespace Infrastructure.InteractionService
{
    // ReSharper disable once UnusedType.Global
    public static class InteractionContextReaderWriter
    {
        // ReSharper disable once UnusedMember.Global
        public static void WriteInteractionContext(this NetworkWriter writer, InteractionContext context)
        {
            context.Write(writer);
        }

        // ReSharper disable once UnusedMember.Global
        public static InteractionContext ReadInteractionContext(this NetworkReader reader)
        {
            return reader.ReadByte().CreateContext(reader);
        }
    }
}
