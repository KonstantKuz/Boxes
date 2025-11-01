using Mirror;
using UnityEngine;
using UnityEngine.Events;

namespace Infrastructure.Network.Components
{
    public class IsServerConditionHelper : NetworkBehaviour
    {
        [SerializeField]
        private UnityEvent<bool> onConditionChanged;

        [SerializeField]
        private bool additionalCondition;

        public override void OnStartClient()
        {
            onConditionChanged.Invoke(isServer && additionalCondition);
        }

        public override void OnStartServer()
        {
            onConditionChanged.Invoke(isServer && additionalCondition);
        }

        public void SetCondition(bool condition)
        {
            additionalCondition = condition;
            onConditionChanged.Invoke(isServer && additionalCondition);
        }
    }
}
