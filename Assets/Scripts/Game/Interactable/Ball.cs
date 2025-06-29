using Infrastructure;
using Infrastructure.InteractionService;
using Mirror;
using UnityEngine;

namespace Game.Interactable
{
    public class Ball : NetworkBehaviour, IInteractable
    {
        private Transform currentCaptureRoot;
        private Rigidbody rigidbody;
        private NetworkRigidbodyExtended networkRigidbody;
        private NetworkTransformExtended networkTransform;

        [SyncVar]
        private BallState ballState;

        private Rigidbody Rigidbody =>
            rigidbody ??= GetComponent<Rigidbody>();

        private NetworkRigidbodyExtended NetworkRigidbody =>
            networkRigidbody ??= GetComponent<NetworkRigidbodyExtended>();

        private NetworkTransformExtended NetworkTransform =>
            networkTransform ??= GetComponent<NetworkTransformExtended>();

        InteractableState IInteractable.State => ballState;

        public override void OnStartServer()
        {
            ballState = new BallState(0);
        }

        // [ClientRpc]
        // private void RpcInteract(InteractionContext interactionContext)
        // {
        //     switch (interactionContext)
        //     {
        //         case KickInteractionContext kickContext:
        //             HandleKick(kickContext);
        //             break;
        //         case CaptureInteractionContext captureContext:
        //             HandleCapture(captureContext);
        //             break;
        //     }
        // }

        private void CmdHandleKick(KickInteractionContext kickContext)
        {
            ballState = new BallState(0);

            NetworkTransform.CmdSetParentImmediately(null, 0);
            NetworkRigidbody.CmdSetIsKinematicImmediately(false);

            Rigidbody.AddForce(kickContext.KickDirection * kickContext.KickForce, ForceMode.Impulse);
        }

        private void CmdHandleCapture(CaptureInteractionContext captureContext)
        {
            ballState = new BallState(captureContext.CaptureRootNetId);

            NetworkRigidbody.CmdSetIsKinematicImmediately(true);

            SetSocketParent(captureContext);

            RpcSetSocketParent(captureContext);
        }

        [ClientRpc]
        private void RpcSetSocketParent(CaptureInteractionContext captureContext)
        {
            SetSocketParent(captureContext);
        }

        private void SetSocketParent(CaptureInteractionContext captureContext)
        {
            if (
                !NetworkClient.spawned.TryGetValue(captureContext.CaptureRootNetId, out NetworkIdentity captureRoot) ||
                !captureRoot.TryGetComponent(out ICaptureInteractionContextRoot captureContextRoot)
            )
            {
                return;
            }

            transform.SetParent(captureContextRoot.Socket);
            transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
        }

        [Command(requiresAuthority = false)]
        void IInteractable.CmdInteract(InteractionContext interactionContext)
        {
            switch (interactionContext)
            {
                case KickInteractionContext kickContext:
                    CmdHandleKick(kickContext);
                    break;
                case CaptureInteractionContext captureContext:
                    CmdHandleCapture(captureContext);
                    break;
            }

            // RpcInteract(interactionContext);
        }
    }
}
