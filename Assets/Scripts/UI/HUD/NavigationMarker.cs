using Infrastructure.CameraService;
using Infrastructure.NavigationService;
using Reflex.Attributes;
using UnityEngine;

namespace UI.HUD
{
    public class NavigationMarker : MonoBehaviour
    {
        [SerializeField]
        private float margin;

        [SerializeField]
        private RectTransform markerTransform;

        private ICameraService cameraService;
        private INavigationService navigationService;

        [Inject]
        private void Construct(ICameraService cameraService, INavigationService navigationService)
        {
            this.cameraService = cameraService;
            this.navigationService = navigationService;
        }

        private void Update()
        {
            Transform target = navigationService.ActiveTarget.CurrentValue;
            markerTransform.gameObject.SetActive(target);

            if (!target)
            {
                return;
            }

            Vector3 screenPositionRaw = cameraService.Camera.WorldToScreenPoint(target.position);
            Vector2 screenPosition = new Vector2(screenPositionRaw.x, screenPositionRaw.y);

            Rect screenRect = new Rect(margin, margin, Screen.width - 2 * margin, Screen.height - 2 * margin);

            if (screenRect.Contains(screenPosition))
            {
                markerTransform.position = screenPosition;
                return;
            }

            Vector3 localTarget = cameraService.Camera.transform.InverseTransformPoint(target.position);
            Vector2 direction = new Vector2(localTarget.x, localTarget.y).normalized;

            Vector2 screenCenter = new Vector2(Screen.width / 2f, Screen.height / 2f);
            Vector2 halfSize = new Vector2(screenRect.width / 2f, screenRect.height / 2f);

            float scaleX = halfSize.x / Mathf.Abs(direction.x);
            float scaleY = halfSize.y / Mathf.Abs(direction.y);
            float scale = Mathf.Min(scaleX, scaleY);

            Vector2 intersection = screenCenter + direction * scale;
            intersection.x = Mathf.Clamp(intersection.x, screenRect.xMin, screenRect.xMax);
            intersection.y = Mathf.Clamp(intersection.y, screenRect.yMin, screenRect.yMax);

            markerTransform.position = intersection;
        }
    }
}
