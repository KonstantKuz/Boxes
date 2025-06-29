using Mirror;
using UnityEngine;

namespace Infrastructure
{
    public class NetworkTransformExtended : NetworkBehaviour
    {
        public void CmdSetParentImmediately(Transform parent, uint parentNetId)
        {
            transform.SetParent(parent);

            CmdSetParent(parentNetId);
        }

        [Command(requiresAuthority = false)]
        public void CmdSetLocalTransform(Vector3 position, Quaternion rotation)
        {
            transform.SetLocalPositionAndRotation(position, rotation);

            RpcSetLocalTransform(position, rotation);
        }

        [ClientRpc]
        private void RpcSetLocalTransform(Vector3 position, Quaternion rotation)
        {
            if (isServer)
            {
                return;
            }

            transform.SetLocalPositionAndRotation(position, rotation);
        }

        [Command(requiresAuthority = false)]
        public void CmdSetParent(uint parentNetId)
        {
            RpcSetParent(netId, parentNetId);
        }

        [ClientRpc]
        private void RpcSetParent(uint childNetId, uint parentNetId)
        {
            if (!NetworkClient.spawned.TryGetValue(childNetId, out NetworkIdentity childIdentity))
            {
                return;
            }

            Transform parent = NetworkClient.spawned.TryGetValue(parentNetId, out NetworkIdentity parentIdentity)
                ? parentIdentity.transform
                : null;

            childIdentity.transform.SetParent(parent, true);
        }
    }
}
