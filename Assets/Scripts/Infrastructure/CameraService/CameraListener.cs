using Reflex.Attributes;
using UnityEngine;
using UnityEngine.Events;

namespace Infrastructure.CameraService
{
    public class CameraListener : MonoBehaviour
    {
        [SerializeField]
        private UnityEvent<Transform> onCameraTransformChanged;

        private ICameraService cameraService;

        [Inject]
        private void Construct(ICameraService cameraService)
        {
            this.cameraService = cameraService;
        }

        private void OnEnable()
        {
            onCameraTransformChanged.Invoke(cameraService.Camera.transform);
        }
    }
}
