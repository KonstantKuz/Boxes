using Infrastructure.Network.Abstract;
using MessagePack;

namespace Gameplay.Interactable.BallInteraction.BallReaction
{
    [MessagePackObject]
    public class DamageableReactionCommand : INetworkCommand
    {
        [Key(0)]
        public byte HitPoints { get; }

        public DamageableReactionCommand(byte hitPoints)
        {
            HitPoints = hitPoints;
        }
    }
}
