using UnityEngine;
using UnityEngine.Events;

namespace Infrastructure.Components
{
    public class Event : MonoBehaviour
    {
        [SerializeField]
        private UnityEvent OnRaised;

        public void Raise()
        {
            OnRaised?.Invoke();
        }
    }
}
