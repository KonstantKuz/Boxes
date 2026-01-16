using Infrastructure.CameraService;
using Reflex.Attributes;
using UnityEngine;
using UnityEngine.Events;

namespace Infrastructure.Components
{
    public class BoundsHelper : MonoBehaviour
    {
        [SerializeField]
        private UnityEvent<bool> onIsVisibleChanged;

        [SerializeField]
        private Collider target;

        private ICameraService cameraService;
        private bool wasVisible;

        [Inject]
        private void Construct(ICameraService cameraService)
        {
            this.cameraService = cameraService;
        }

        private void Update()
        {
            bool isVisible = cameraService.IsVisible(target.bounds, out _);
            if (wasVisible != isVisible)
            {
                wasVisible = isVisible;
                onIsVisibleChanged?.Invoke(isVisible);
            }
        }
    }
}
