using System;
using Infrastructure;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Gameplay.Interactable.BallInteraction
{
    [CreateAssetMenu(
        fileName = nameof(BallInteractionConfig),
        menuName = GlobalParams.ConfigPath + nameof(BallInteractionConfig)
    )]
    public class BallInteractionConfig : ScriptableObject
    {
        [TitleGroup("Base params")]
        [SerializeField]
        private float interactionDistance;

        [SerializeField]
        private float minSpeed;

        [SerializeField]
        private float maxSpeed;

        [SerializeField]
        private float maxHeight;

        [SerializeField]
        private float heightDampingStrength;

        [SerializeField]
        private float outOfBoundsPullForce;

        [SerializeField]
        private float kickWindowTime;

        [TitleGroup("Status params")]
        [SerializeField]
        private int statusKicksCount;

        [SerializeField]
        private float kickSpeedModifier;

        [SerializeField]
        private float statusDampingDistance;

        [TitleGroup("Auto capture")]
        [SerializeField]
        private float autoCaptureTime;

        [SerializeField]
        private float autoCaptureRechargeTime;

        [TitleGroup("Auto attraction")]
        [SerializeField]
        private float attractionRadius;

        [SerializeField]
        private float attractionForce;

        [SerializeField]
        private float attractionMinRequiredSpeed;

        [TitleGroup("Status reset conditions")]
        [SerializeField]
        private ResetCondition resetConditions;

        public float InteractionDistance => interactionDistance;
        public float KickSpeedModifier => kickSpeedModifier;
        public float KickWindowTime => kickWindowTime;
        public float MinSpeed => minSpeed;
        public float MaxSpeed => maxSpeed;
        public float MaxHeight => maxHeight;
        public float HeightDampingStrength => heightDampingStrength;
        public float StatusDampingDistance => statusDampingDistance;
        public float OutOfBoundsPullForce => outOfBoundsPullForce;
        public int StatusKicksCount => statusKicksCount;
        public float AttractionRadius => attractionRadius;
        public float AttractionForce => attractionForce;
        public float AttractionMinRequiredSpeed => attractionMinRequiredSpeed;
        public float AutoCaptureTime => autoCaptureTime;
        public float AutoCaptureRechargeTime => autoCaptureRechargeTime;
        public ResetCondition ResetConditions => resetConditions;
    }

    [Flags]
    public enum ResetCondition
    {
        None = 0,
        SamePlayerKick = 1,
        Hold = 2,
        Collision = 4,
        Reaction = 8
    }
}
