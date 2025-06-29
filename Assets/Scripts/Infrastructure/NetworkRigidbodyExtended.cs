using Mirror;
using UnityEngine;

namespace Infrastructure
{
    public class NetworkRigidbodyExtended : NetworkBehaviour
    {
        private Rigidbody rigidbody;
        private Rigidbody Rigidbody => rigidbody ??= GetComponent<Rigidbody>();

        public void CmdSetIsKinematicImmediately(bool value)
        {
            Rigidbody.isKinematic = value;

            CmdSetIsKinematic(value);
        }

        [Command(requiresAuthority = false)]
        public void CmdSetIsKinematic(bool value)
        {
            RpcSetIsKinematic(value);
        }

        [ClientRpc]
        private void RpcSetIsKinematic(bool value)
        {
            if (isServer)
            {
                return;
            }

            Rigidbody.isKinematic = value;
        }
    }
}
