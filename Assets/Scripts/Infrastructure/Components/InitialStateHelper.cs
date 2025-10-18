using UnityEngine;

namespace Infrastructure.Components
{
    public class InitialStateHelper : MonoBehaviour
    {
        private bool initialIsActive;
        private Vector3 initialPosition;
        private Quaternion initialRotation;

        private void OnValidate()
        {
            initialIsActive = gameObject.activeSelf;
            initialPosition = transform.position;
            initialRotation = transform.rotation;
        }

        public void ResetState()
        {
            transform.position = initialPosition;
            transform.rotation = initialRotation;
            gameObject.SetActive(initialIsActive);
        }
    }
}
