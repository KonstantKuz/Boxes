using System;
using Infrastructure;
using Infrastructure.InteractionService;
using Mirror;
using Network;

namespace Game.Interactable
{
    // ReSharper disable once UnusedType.Global
    public static class InteractionTypeExtensions
    {
        // ReSharper disable once UnusedMember.Global
        public static InteractionContext CreateContext(this byte typeId, NetworkReader reader)
        {
            Type type = TypeByteMapper<InteractionContext>.GetTypeFromByte(typeId);

            return type.Name switch
            {
                nameof(KickInteractionContext) => KickInteractionContext.Read(reader),
                nameof(CaptureInteractionContext) => CaptureInteractionContext.Read(reader),
                _ => throw new ArgumentException($"Unknown interaction context type: {type.Name}")
            };
        }
    }

}
