using Mirror;
using UnityEngine;

namespace Infrastructure
{
    public class NetworkTransformExtended : NetworkBehaviour
    {
        private NetworkTransformReliable networkTransformReliable;

        public NetworkTransformReliable Reliable =>
            networkTransformReliable ??= GetComponent<NetworkTransformReliable>();

        [Command(requiresAuthority = false)]
        public void CmdSetLocalTransform(Vector3 position, Quaternion rotation)
        {
            RpcSetLocalTransform(position, rotation);
        }

        [ClientRpc]
        private void RpcSetLocalTransform(Vector3 position, Quaternion rotation)
        {
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

        [Command(requiresAuthority = false)]
        public void CmdSetSyncDirection(SyncDirection syncDirection)
        {
            RpcSetSyncDirection(syncDirection);
        }

        [ClientRpc]
        private void RpcSetSyncDirection(SyncDirection syncDirection)
        {
            Reliable.syncDirection = syncDirection;
        }
    }
}
