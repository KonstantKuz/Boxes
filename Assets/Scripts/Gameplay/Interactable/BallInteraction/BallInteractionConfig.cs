using UnityEngine;

namespace Gameplay.Interactable.BallInteraction
{
    [CreateAssetMenu(fileName = "BallInteractionConfig", menuName = "Configs/BallInteractionConfig")]
    public class BallInteractionConfig : ScriptableObject
    {
        [field:SerializeField]
        public float CaptureMinDistance { get; private set; }

        [field:SerializeField]
        public float KickMinDistance { get; private set; }

        [field:SerializeField]
        public float KickSpeedModifier { get; private set; }

        [field:SerializeField]
        public float MinSpeed { get; private set; }

        [field:SerializeField]
        public float MaxSpeed { get; private set; }

        [field:SerializeField]
        public float MaxHeight { get; private set; }

        [field:SerializeField]
        public float DampingStrength { get; private set; }

        [field:SerializeField]
        public float DecayDistance { get; private set; }

        [field:SerializeField]
        public float OutOfBoundsPullForce { get; private set; }

        [field:SerializeField]
        public int MaxKicksCount { get; private set; }
    }
}
