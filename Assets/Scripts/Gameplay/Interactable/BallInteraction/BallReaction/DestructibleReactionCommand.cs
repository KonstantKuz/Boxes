using Infrastructure.Network.Abstract;
using MessagePack;

namespace Gameplay.Interactable.BallInteraction.BallReaction
{
    [MessagePackObject]
    public class DestructibleReactionCommand : INetworkCommand
    {
        [Key(0)]
        public byte HitPoints { get; }

        public DestructibleReactionCommand(byte hitPoints)
        {
            HitPoints = hitPoints;
        }
    }
}
