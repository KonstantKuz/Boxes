using UnityEngine;
using UnityEngine.Events;

namespace Infrastructure.Components
{
    public class GameObjectEvent : MonoBehaviour
    {
        [SerializeField]
        private UnityEvent OnRaised;

        public void Raise()
        {
            OnRaised?.Invoke();
        }
    }
}
