using System;
using Infrastructure;
using Infrastructure.InteractionService.Abstract;
using Mirror;

namespace Gameplay.Interactable.BallInteraction
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
            writer.WriteByte(TypeByteMapper.GetByteFromType<CaptureInteractionContext>());
            writer.WriteUInt(CaptureRootNetId);
        }
    }
}
