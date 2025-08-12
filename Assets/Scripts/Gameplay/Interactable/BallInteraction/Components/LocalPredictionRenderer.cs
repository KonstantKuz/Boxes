using Gameplay.Interactable.BallInteraction.Abstract;
using Reflex.Attributes;
using UnityEngine;

namespace Gameplay.Interactable.BallInteraction.Components
{
    [RequireComponent(typeof(Ball))]
    public class LocalPredictionRenderer : MonoBehaviour
    {
        [SerializeField]
        private Transform directionRenderer;

        private IBallInteractionMediator ballInteractionMediator;
        private Ball ball;

        [Inject]
        private void Construct(IBallInteractionMediator ballInteractionMediator)
        {
            this.ballInteractionMediator = ballInteractionMediator;
        }

        private void Awake()
        {
            ball = GetComponent<Ball>();
            directionRenderer.SetParent(null);
        }

        private void Update()
        {
            if (ball && directionRenderer)
            {
                bool isVisible = ballInteractionMediator.IsPredictionVisible(out Vector3 direction);
                directionRenderer.gameObject.SetActive(isVisible);
                directionRenderer.rotation = Quaternion.LookRotation(direction);
                directionRenderer.position = ball.transform.position;
            }
        }
    }
}
