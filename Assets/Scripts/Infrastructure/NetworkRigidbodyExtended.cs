using Mirror;
using UnityEngine;

namespace Infrastructure
{
    public class NetworkRigidbodyExtended : NetworkBehaviour
    {
        private Rigidbody rigidbody;
        private NetworkRigidbodyReliable networkRigidbodyReliable;

        private Rigidbody Rigidbody => rigidbody ??= GetComponent<Rigidbody>();
        private NetworkRigidbodyReliable Reliable =>
            networkRigidbodyReliable ??= gameObject.GetComponent<NetworkRigidbodyReliable>();

        [Command(requiresAuthority = false)]
        public void CmdSetEnabled(bool value)
        {
            RpcSetEnabled(value);
        }

        [ClientRpc]
        private void RpcSetEnabled(bool value)
        {
            Reliable.enabled = value;
        }

        [Command(requiresAuthority = false)]
        public void CmdSetIsKinematic(bool value)
        {
            RpcSetIsKinematic(value);
        }

        [ClientRpc]
        private void RpcSetIsKinematic(bool value)
        {
            Rigidbody.isKinematic = value;
        }

        [Command(requiresAuthority = false)]
        public void CmdAddForce(Vector3 force, ForceMode mode)
        {
            RpcAddForce(force, mode);
        }

        [ClientRpc]
        private void RpcAddForce(Vector3 force, ForceMode mode)
        {
            Rigidbody.AddForce(force, mode);
        }

    }
}
