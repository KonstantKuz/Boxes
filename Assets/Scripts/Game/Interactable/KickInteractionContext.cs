using System;
using Infrastructure;
using Infrastructure.InteractionService;
using Mirror;
using UnityEngine;

namespace Game.Interactable
{
    [Serializable]
    public class KickInteractionContext : InteractionContext
    {
        public readonly uint InitiatorNetId;
        public readonly Vector3 KickDirection;
        public readonly float KickForce;

        public KickInteractionContext()
        {

        }

        public KickInteractionContext(uint initiatorNetId, Vector3 kickDirection, float kickForce = 0)
        {
            InitiatorNetId = initiatorNetId;
            KickDirection = kickDirection;
            KickForce = kickForce;
        }

        public static InteractionContext Read(NetworkReader reader)
        {
            return new KickInteractionContext(reader.ReadUInt(), reader.ReadVector3(), reader.ReadFloat());
        }

        public override void Write(NetworkWriter writer)
        {
            writer.WriteByte(TypeByteMapper<InteractionContext>.GetByteFromType(GetType()));
            writer.WriteUInt(InitiatorNetId);
            writer.WriteVector3(KickDirection);
            writer.WriteFloat(KickForce);
        }
    }
}
