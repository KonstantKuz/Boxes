using Mirror;
using UnityEngine;

namespace Infrastructure.Network.Components
{
    public class NetworkInitialStateHelper : NetworkBehaviour
    {
        [SyncVar]
        private bool initialIsActive;

        [SyncVar]
        private Vector3 initialPosition;

        [SyncVar]
        private Quaternion initialRotation;

        protected override void OnValidate()
        {
            base.OnValidate();

            initialIsActive = gameObject.activeSelf;
            initialPosition = transform.position;
            initialRotation = transform.rotation;
        }

        public override void OnStartServer()
        {
            CmdResetState();
        }

        [Command(requiresAuthority = false)]
        public void CmdSetActive(bool value)
        {
            gameObject.SetActive(value);
            RpcSetActive(value);
        }

        [ClientRpc]
        private void RpcSetActive(bool value)
        {
            gameObject.SetActive(value);
        }

        [Command(requiresAuthority = false)]
        public void CmdResetState()
        {
            if (TryGetComponent(out NetworkTransformBase networkTransform))
            {
                networkTransform.ServerTeleport(initialPosition, initialRotation);
            }

            transform.position = initialPosition;
            transform.rotation = initialRotation;
            gameObject.SetActive(initialIsActive);

            RpcResetState();
        }

        [ClientRpc]
        public void RpcResetState()
        {
            transform.position = initialPosition;
            transform.rotation = initialRotation;
            gameObject.SetActive(initialIsActive);
        }
    }
}
