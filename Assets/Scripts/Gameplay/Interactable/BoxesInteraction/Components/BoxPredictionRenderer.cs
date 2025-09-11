using Gameplay.Interactable.BoxesInteraction.Abstract;
using Reflex.Attributes;
using UnityEngine;

namespace Gameplay.Interactable.BoxesInteraction.Components
{
    [RequireComponent(typeof(Box))]
    public class BoxPredictionRenderer : MonoBehaviour
    {
        [SerializeField]
        private Rigidbody rigidbody;

        [SerializeField]
        private Transform directionRenderer;

        private IBoxesInteractionMediator boxInteractionMediator;
        private Box box;

        [Inject]
        private void Construct(IBoxesInteractionMediator boxInteractionMediator)
        {
            this.boxInteractionMediator = boxInteractionMediator;
        }

        private void Awake()
        {
            box = GetComponent<Box>();
            directionRenderer.SetParent(null);
        }

        private void Update()
        {
            if (box && directionRenderer && box.State.HasHolder)
            {
                bool isVisible = boxInteractionMediator.IsPredictionVisible(out Vector3 targetPosition);
                directionRenderer.gameObject.SetActive(isVisible);
                directionRenderer.position = targetPosition;
            }
            else
            {
                directionRenderer.gameObject.SetActive(box.Rigidbody.velocity.magnitude > 0);
                directionRenderer.position =
                    boxInteractionMediator.CalculateLandingPoint(box.Rigidbody.position, box.Rigidbody.velocity);
            }
        }
    }
}
