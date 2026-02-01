using Gameplay.Interactable.BallInteraction.Abstract;
using Gameplay.Interactable.BallInteraction.State;
using Reflex.Attributes;
using UnityEngine;

namespace Gameplay.Interactable.BallInteraction.Components
{
    [RequireComponent(typeof(Ball))]
    public class BallPredictionRenderer : MonoBehaviour
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
                IBallInteractionInitiator controllingInitiator = GetControllingInitiator();

                if (controllingInitiator != null)
                {
                    bool isVisible = ballInteractionMediator.IsPredictionVisible(controllingInitiator, out Vector3 direction);
                    directionRenderer.gameObject.SetActive(isVisible);
                    directionRenderer.rotation = Quaternion.LookRotation(direction);
                    directionRenderer.position = ball.transform.position;
                }
                else
                {
                    directionRenderer.gameObject.SetActive(false);
                }
            }
        }

        private IBallInteractionInitiator GetControllingInitiator()
        {
            var state = ball.StateHolder.GetState();

            if (state.HasHolder)
            {
                ballInteractionMediator.Initiators.TryGetValue(state.HolderNetId, out IBallInteractionInitiator holder);
                return holder;
            }

            if (state.LastActionType == BallActionType.Capture)
            {
                ballInteractionMediator.Initiators.TryGetValue(state.OwnerNetId, out IBallInteractionInitiator owner);
                return owner;
            }

            return null;
        }
    }
}
