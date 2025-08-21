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

            Vector3 screenPoint =
                cameraService.Camera.WorldToScreenPoint(target.position);

            if (screenPoint.z < 0)
            {
                screenPoint.x = Screen.width - screenPoint.x;
                screenPoint.y = Screen.height - screenPoint.y;
            }

            float x = Mathf.Clamp(screenPoint.x, margin, Screen.width - margin);
            float y = Mathf.Clamp(screenPoint.y, margin, Screen.height - margin);

            markerTransform.position = new Vector3(x, y, 0f);
        }
    }
}
