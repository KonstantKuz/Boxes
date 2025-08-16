using UnityEngine;

namespace Gameplay.Interactable.BallInteraction
{
    public class BallInteractionDebug : MonoBehaviour
    {
        [SerializeField]
        private Color interactionDistanceColor = Color.green;

        [SerializeField]
        private BallInteractionConfig config;

        private void OnDrawGizmos()
        {
            if (!config)
            {
                return;
            }

            Color defaultColor = Gizmos.color;

            Gizmos.color = interactionDistanceColor;
            Gizmos.DrawWireSphere(transform.position, config.InteractionDistance);

            Gizmos.color = defaultColor;
        }
    }
}
