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
    public class CameraControllerInput: CameraInput
    {
        public bool invertHorizontalInput = false;
        public bool invertVerticalInput = false;

        public float mouseInputMultiplier = 0.01f;

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
            connectionStateSubscription = networkManager.ConnectionState.Subscribe(UpdateActions);

            NetworkIdentity networkIdentity = GetComponent<NetworkIdentity>();
            if (networkIdentity != null && networkIdentity.netId != 0 && networkIdentity.isLocalPlayer)
            {
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
            if (networkIdentity == null || networkIdentity.netId == 0 || !networkIdentity.isOwned)
            {
                return;
            }

            GameInputActions newActions = inputService.GetInput(networkIdentity.netId);
            actions = newActions ?? actions;
        }

        public override float GetHorizontalCameraInput()
        {
            if (actions == null) return 0f;

            float input = actions.DefaultContext.Aim.ReadValue<Vector2>().x;

            if(Time.timeScale > 0f && Time.deltaTime > 0f)
            {
                input /= Time.deltaTime;
                input *= Time.timeScale;
            }
            else
                input = 0f;

            input *= mouseInputMultiplier;

            if(invertHorizontalInput)
                input *= -1f;

            return input;
        }

        public override float GetVerticalCameraInput()
        {
            if (actions == null) return 0f;

            float input = -actions.DefaultContext.Aim.ReadValue<Vector2>().y;

            if(Time.timeScale > 0f && Time.deltaTime > 0f)
            {
                input /= Time.deltaTime;
                input *= Time.timeScale;
            }
            else
                input = 0f;

            input *= mouseInputMultiplier;

            if(invertVerticalInput)
                input *= -1f;

            return input;
        }
    }
}
