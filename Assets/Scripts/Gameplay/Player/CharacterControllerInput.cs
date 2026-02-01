using System;
using CMF;
using Infrastructure.InputService.Abstract;
using Infrastructure.Network.Abstract;
using Infrastructure.Network.State;
using Mirror;
using R3;
using Reflex.Attributes;
using UnityEngine;

namespace Gameplay.Player
{
    public class CharacterControllerInput : CharacterInput
    {
        [SerializeField]
        private TurnTowardControllerVelocity turnController;

        private IInputService inputService;
        private INetworkManager networkManager;
        private GameInputActions actions;
        private IDisposable connectionStateSubscription;

        [Inject]
        private void Construct(IInputService inputService, INetworkManager networkManager)
        {
            this.inputService = inputService;
            this.networkManager = networkManager;
        }

        private void Start()
        {
            Debug.Log("CharacterControllerInput.Start: Subscribing to ConnectionState");
            connectionStateSubscription = networkManager.ConnectionState.Subscribe(UpdateActions);

            NetworkIdentity networkIdentity = GetComponent<NetworkIdentity>();
            if (networkIdentity != null && networkIdentity.netId != 0 && networkIdentity.isLocalPlayer)
            {
                Debug.Log($"CharacterControllerInput.Start: Immediately updating actions for netId={networkIdentity.netId}");
                UpdateActions(networkManager.ConnectionState.CurrentValue);
            }
        }

        private void OnDestroy()
        {
            connectionStateSubscription?.Dispose();
        }

        private void UpdateActions(ConnectionState state)
        {
            NetworkIdentity networkIdentity = GetComponent<NetworkIdentity>();
            if (networkIdentity == null || networkIdentity.netId == 0)
            {
                Debug.LogWarning($"CharacterControllerInput.UpdateActions: networkIdentity={networkIdentity}, netId={networkIdentity?.netId}");
                return;
            }

            if (!networkIdentity.isOwned)
            {
                Debug.LogWarning($"CharacterControllerInput.UpdateActions: Not owned, netId={networkIdentity.netId}");
                return;
            }

            Debug.Log($"CharacterControllerInput.UpdateActions: Getting input for netId={networkIdentity.netId}");
            GameInputActions newActions = inputService.GetInput(networkIdentity.netId);
            if (newActions != null)
            {
                actions = newActions;
                Debug.Log($"CharacterControllerInput.UpdateActions: Actions updated for netId={networkIdentity.netId}");
            }
            else
            {
                Debug.LogError($"CharacterControllerInput.UpdateActions: GetInput returned null for netId={networkIdentity.netId}");
            }
        }

        public override float GetHorizontalMovementInput()
        {
            if (actions == null)
            {
                Debug.LogWarning("CharacterControllerInput: actions is null in GetHorizontalMovementInput");
                return 0f;
            }
            return actions.DefaultContext.Move.ReadValue<Vector2>().x;
        }

        public override float GetVerticalMovementInput()
        {
            if (actions == null) return 0f;
            return actions.DefaultContext.Move.ReadValue<Vector2>().y;
        }

        public override bool IsJumpKeyPressed()
        {
            if (actions == null) return false;
            return actions.DefaultContext.Jump.IsPressed();
        }

        private void Update()
        {
            if (actions == null) return;
            Vector2 lookDirection = actions.DefaultContext.Aim.ReadValue<Vector2>();
            if (turnController != null)
            {
                turnController.SetTargetDirection(lookDirection);
            }
        }
    }
}
