using Game.Interactable.Abstract;
using Game.Interactable.NetworkData;
using Infrastructure;
using Infrastructure.InteractionService;
using Mirror;
using UnityEngine;

namespace Game.Interactable
{
    public class Ball : NetworkBehaviour, IInteractable
    {
        private const float InterpolationSpeed = 20f;

        private Rigidbody rigidbody;
        private NetworkRigidbodyExtended networkRigidbody;
        private NetworkTransformExtended networkTransform;
        private Transform socketTransform;

        [SyncVar]
        private BallState ballState;

        private NetworkRigidbodyExtended NetworkRigidbody =>
            networkRigidbody ??= GetComponent<NetworkRigidbodyExtended>();

        private NetworkTransformExtended NetworkTransform =>
            networkTransform ??= GetComponent<NetworkTransformExtended>();

        InteractableState IInteractable.State => ballState;

        public override void OnStartServer()
        {
            ballState = new BallState(0);
        }

        private void CmdHandleKick(KickInteractionContext kickContext)
        {
            ballState = new BallState(0);

            NetworkRigidbody.CmdSetEnabled(true);
            NetworkRigidbody.CmdSetIsKinematic(false);
            NetworkRigidbody.CmdAddForce(kickContext.KickDirection * kickContext.KickForce, ForceMode.Impulse);
        }

        private void CmdHandleCapture(CaptureInteractionContext captureContext)
        {
            if (!NetworkClient.spawned.TryGetValue(captureContext.CaptureRootNetId, out NetworkIdentity captureRoot))
            {
                return;
            }

            ballState = new BallState(captureContext.CaptureRootNetId);
            NetworkRigidbody.CmdSetIsKinematic(true);
            NetworkRigidbody.CmdSetEnabled(false);
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

        private void Update()
        {
            uint ownerNetId = ballState?.OwnerNetId ?? 0;

            if (
                !NetworkClient.spawned.TryGetValue(ownerNetId, out NetworkIdentity owner) ||
                !owner.TryGetComponent(out ICaptureInteractionContextRoot captureContextRoot)
            )
            {
                return;
            }

            transform.position = Vector3.Lerp(
                transform.position, captureContextRoot.Socket.position, InterpolationSpeed * Time.deltaTime
            );
        }
    }
}
