using Gameplay.Interactable.Abstract;
using Infrastructure;
using Infrastructure.InteractionService.Abstract;
using Infrastructure.Network.Abstract;
using Mirror;
using Reflex.Attributes;
using UnityEngine;

namespace Gameplay.Interactable.BallInteraction
{
    public class Ball : NetworkBehaviour, IInteractable
    {
        private const float InterpolationSpeed = 20f;

        [SerializeField]
        private BallStateHolder ballStateHolder;

        private INetworkService networkService;

        private Rigidbody rigidbody;
        private NetworkRigidbodyExtended networkRigidbody;
        private NetworkTransformExtended networkTransform;
        private Transform socketTransform;

        private NetworkRigidbodyExtended NetworkRigidbody =>
            networkRigidbody ??= GetComponent<NetworkRigidbodyExtended>();

        private NetworkTransformExtended NetworkTransform =>
            networkTransform ??= GetComponent<NetworkTransformExtended>();

        private INetworkStateHolder<BallState> BallStateHolder => ballStateHolder;

        InteractableState IInteractable.State => BallStateHolder.State;

        [Inject]
        private void Construct(INetworkService networkService)
        {
            this.networkService = networkService;
        }

        public override void OnStartServer()
        {
            networkService.ObserveToExecute<KickInteractionContext>(CmdHandleKick);
            networkService.ObserveToExecute<CaptureInteractionContext>(CmdHandleCapture);
        }

        private void CmdHandleKick(KickInteractionContext kickContext)
        {
            BallStateHolder.WriteState(BallState.Default);

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

            BallStateHolder.WriteState(new BallState(captureContext.CaptureRootNetId));

            NetworkRigidbody.CmdSetIsKinematic(true);
            NetworkRigidbody.CmdSetEnabled(false);
        }

        void IInteractable.Interact(InteractionContext interactionContext)
        {
            switch (interactionContext)
            {
                case KickInteractionContext kickContext:
                    networkService.SendCommand(kickContext);
                    break;
                case CaptureInteractionContext captureContext:
                    networkService.SendCommand(captureContext);
                    break;
            }
        }

        private void Update()
        {
            uint ownerNetId = BallStateHolder.State?.OwnerNetId ?? 0;

            if (
                !NetworkClient.spawned.TryGetValue(ownerNetId, out NetworkIdentity owner) ||
                !owner.TryGetComponent(out IBallInteractionInitiator ballInteractionInitiator)
            )
            {
                return;
            }

            // transform.position = Vector3.Lerp(
            //     transform.position, ballInteractionInitiator.BallSocket.position, InterpolationSpeed * Time.deltaTime
            // );

            transform.position = ballInteractionInitiator.BallSocket.position;
        }
    }
}
