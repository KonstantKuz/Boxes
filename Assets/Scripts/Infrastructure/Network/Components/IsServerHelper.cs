using Mirror;
using UnityEngine;
using UnityEngine.Events;

namespace Infrastructure.Network.Components
{
    public class IsServerHelper : NetworkBehaviour
    {
        [SerializeField]
        private UnityEvent<bool> IsServer;

        public override void OnStartClient()
        {
            IsServer.Invoke(isServer);
        }

        public override void OnStartServer()
        {
            IsServer.Invoke(isServer);
        }

        public void AddCondition(bool condition)
        {
            IsServer.Invoke(isServer & condition);
        }
    }
}
