using System;
using Infrastructure;
using Infrastructure.InteractionService;
using Mirror;

namespace Game.Interactable.NetworkData
{
    [Serializable]
    public class CaptureInteractionContext : InteractionContext
    {
        public readonly uint CaptureRootNetId;

        public CaptureInteractionContext()
        {

        }

        public CaptureInteractionContext(uint captureRootNetId)
        {
            CaptureRootNetId = captureRootNetId;
        }

        public static InteractionContext Read(NetworkReader reader)
        {
            return new CaptureInteractionContext(reader.ReadUInt());
        }

        public override void Write(NetworkWriter writer)
        {
            writer.WriteByte(TypeByteMapper<InteractionContext>.GetByteFromType(GetType()));
            writer.WriteUInt(CaptureRootNetId);
        }
    }
}
