using UnityEngine;

namespace Gameplay.Interactable.BallInteraction
{
    public class BallInteractionDebug : MonoBehaviour
    {
        [SerializeField]
        private Color captureMinDistanceColor = Color.red;

        [SerializeField]
        private Color kickMinDistanceColor = Color.green;

        [SerializeField]
        private BallInteractionConfig config;

        private void OnDrawGizmos()
        {
            if (!config)
            {
                return;
            }

            Color defaultColor = Gizmos.color;

            Gizmos.color = captureMinDistanceColor;
            Gizmos.DrawWireSphere(transform.position, config.CaptureMinDistance);

            Gizmos.color = kickMinDistanceColor;
            Gizmos.DrawWireSphere(transform.position, config.KickMinDistance);

            Gizmos.color = defaultColor;
        }
    }
}
