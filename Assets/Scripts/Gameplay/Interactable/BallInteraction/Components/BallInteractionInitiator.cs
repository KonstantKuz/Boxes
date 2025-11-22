using System;
using Gameplay.Interactable.Abstract;
using Gameplay.Interactable.BallInteraction.Abstract;
using Gameplay.Interactable.BallInteraction.State;
using R3;
using Reflex.Attributes;
using UnityEngine;
using UnityEngine.Events;

namespace Gameplay.Interactable.BallInteraction.Components
{
    public class BallInteractionInitiator : InteractionInitiatorBase, IBallInteractionInitiator
    {
        [SerializeField]
        private UnityEvent<float> onKickSideChanged;

        [SerializeField]
        private UnityEvent onBallKicked;

        [SerializeField]
        private Transform kickDirectionRoot;

        [SerializeField]
        private Transform ballSocket;

        private IBallInteractionMediator ballInteractionMediator;
        private IDisposable stateSubscription;
        private uint lastProcessedActionId;

        uint IBallInteractionInitiator.NetId => netId;
        Transform IBallInteractionInitiator.BallSocket => ballSocket;
        Vector3 IBallInteractionInitiator.Position => transform.position;
        Vector3 IBallInteractionInitiator.KickDirection => kickDirectionRoot.forward;

        [Inject]
        private void Construct(IBallInteractionMediator ballInteractionMediator)
        {
            this.ballInteractionMediator = ballInteractionMediator;
        }

        public override void OnStartClient()
        {
            ballInteractionMediator.RegisterInitiator(this, isLocalPlayer);

            stateSubscription = ballInteractionMediator.BallState.Subscribe(OnStateChanged);
        }

        public override void OnStopClient()
        {
            stateSubscription?.Dispose();
        }

        private void OnStateChanged(BallSharedState state)
        {
            if (state.LastActionId == lastProcessedActionId)
            {
                return;
            }

            if (state.LastActionType != BallActionType.Kick || state.LastKickInitiatorNetId != netId)
            {
                lastProcessedActionId = state.LastActionId;
                return;
            }

            float kickSide = 1f;
            if (ballInteractionMediator.Ball != null)
            {
                Vector3 localPosition =
                    ballSocket.InverseTransformPoint(ballInteractionMediator.Ball.transform.position);
                kickSide = localPosition.x >= 0 ? 1f : -1f;
            }

            onKickSideChanged?.Invoke(kickSide);
            onBallKicked?.Invoke();

            lastProcessedActionId = state.LastActionId;
        }
    }
}
