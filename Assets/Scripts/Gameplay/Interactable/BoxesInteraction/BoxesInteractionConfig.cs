using Infrastructure;
using UnityEngine;

namespace Gameplay.Interactable.BoxesInteraction
{
    [CreateAssetMenu(
        fileName = nameof(BoxesInteractionConfig),
        menuName = GlobalParams.ConfigPath + nameof(BoxesInteractionConfig)
    )]
    public class BoxesInteractionConfig : ScriptableObject
    {
        [SerializeField]
        private float interactionDistance;

        [SerializeField]
        private Vector2 throwForce;

        [SerializeField]
        private float holderSpeedModifier;

        [SerializeField]
        private float extraGravity;

        [SerializeField]
        private float predictionRaycastHeight;

        public float InteractionDistance => interactionDistance;
        public Vector2 ThrowForce => throwForce;
        public float HolderSpeedModifier => holderSpeedModifier;
        public float ExtraGravity => extraGravity;
    }
}
