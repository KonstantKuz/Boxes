using UnityEngine;
using UnityEngine.Events;

namespace Infrastructure.Components
{
    public class BoolEvent : MonoBehaviour
    {
        [SerializeField]
        private UnityEvent<bool> OnValueChanged;

        [SerializeField]
        private UnityEvent<bool> OnInvertedValueChanged;

        public void Raise(bool value)
        {
            OnValueChanged?.Invoke(value);
            OnInvertedValueChanged?.Invoke(!value);
        }
    }
}
