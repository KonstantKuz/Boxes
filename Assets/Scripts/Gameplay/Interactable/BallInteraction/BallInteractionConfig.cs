using System;
using UnityEngine;

namespace Gameplay.Interactable.BallInteraction
{
    [CreateAssetMenu(fileName = "BallInteractionConfig", menuName = "Configs/BallInteractionConfig")]
    public class BallInteractionConfig : ScriptableObject
    {
        [SerializeField]
        private float captureMinDistance;

        [SerializeField]
        private float kickMinDistance;

        [SerializeField]
        private float kickSpeedModifier;

        [SerializeField]
        private float kickWindowTime;

        [SerializeField]
        private float minSpeed;

        [SerializeField]
        private float maxSpeed;

        [SerializeField]
        private float maxHeight;

        [SerializeField]
        private float dampingStrength;

        [SerializeField]
        private float decayDistance;

        [SerializeField]
        private float outOfBoundsPullForce;

        [SerializeField]
        private int maxKicksCount;

        [SerializeField]
        private ResetCondition resetConditions;

        public float CaptureMinDistance => captureMinDistance;
        public float KickMinDistance => kickMinDistance;
        public float KickSpeedModifier => kickSpeedModifier;
        public float KickWindowTime => kickWindowTime;
        public float MinSpeed => minSpeed;
        public float MaxSpeed => maxSpeed;
        public float MaxHeight => maxHeight;
        public float DampingStrength => dampingStrength;
        public float DecayDistance => decayDistance;
        public float OutOfBoundsPullForce => outOfBoundsPullForce;
        public int MaxKicksCount => maxKicksCount;
        public ResetCondition ResetConditions => resetConditions;
    }

    [Flags]
    public enum ResetCondition
    {
        None,
        SamePlayerKick,
        Capture
    }
}
