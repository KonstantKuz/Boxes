using UnityEngine;
using UnityEngine.Events;

namespace Infrastructure.Components
{
    public class BoolEvent : MonoBehaviour
    {
        [SerializeField]
        private UnityEvent onTrue;

        [SerializeField]
        private UnityEvent onFalse;

        [SerializeField]
        private UnityEvent<bool> OnValueChanged;

        [SerializeField]
        private UnityEvent<bool> OnInvertedValueChanged;

        public void Raise(bool value)
        {
            if (value)
            {
                onTrue.Invoke();
            }
            else
            {
                onFalse.Invoke();
            }

            OnValueChanged?.Invoke(value);
            OnInvertedValueChanged?.Invoke(!value);
        }
    }
}
