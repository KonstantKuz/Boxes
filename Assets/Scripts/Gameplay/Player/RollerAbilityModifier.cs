using System;
using Infrastructure.InputService.Abstract;
using Infrastructure.Network.Abstract;
using Infrastructure.Network.State;
using Infrastructure.QuestService;
using Infrastructure.QuestService.Abstract;
using Mirror;
using R3;
using Reflex.Attributes;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

namespace Gameplay.Player
{
    public class RollerAbilityModifier : NetworkBehaviour
    {
        [SerializeField]
        private UnityEvent<bool> onIsActiveChanged;

        [SerializeField]
        private UnityEvent<float> onSpeedModifierChanged;

        [SerializeField]
        private UnityEvent<float> onFrictionModifierChanged;

        [SerializeField]
        private RollerAbilityConfig config;

        private IInputService inputService;
        private INetworkManager networkManager;
        private IQuestService questService;

        private IDisposable questSubscription;
        private IDisposable connectionStateSubscription;
        private GameInputActions selfInput;
        private bool isModifierAvailable;
        private bool isModifierActive;

        [Inject]
        private void Construct(IInputService inputService, INetworkManager networkManager, IQuestService questService)
        {
            this.inputService = inputService;
            this.networkManager = networkManager;
            this.questService = questService;
        }

        public override void OnStartClient()
        {
            if (isOwned)
            {
                connectionStateSubscription = networkManager.ConnectionState.Subscribe(OnConnectionStateChanged);
            }
        }

        public override void OnStopClient()
        {
            connectionStateSubscription?.Dispose();

            if (selfInput != null)
            {
                selfInput.DefaultContext.Boost.performed -= ToggleModifier;
            }
        }

        private void OnEnable()
        {
            questSubscription = questService.ActiveQuest.Subscribe(OnActiveQuestChanged);
        }

        private void OnDisable()
        {
            questSubscription?.Dispose();
            onSpeedModifierChanged.Invoke(config.SpeedModifier);
        }

        private void OnConnectionStateChanged(ConnectionState state)
        {
            if (netId == 0)
            {
                return;
            }

            if (selfInput != null)
            {
                selfInput.DefaultContext.Boost.performed -= ToggleModifier;
                selfInput = null;
            }

            GameInputActions newActions = inputService.GetInput(netId);
            if (newActions != null)
            {
                selfInput = newActions;
                selfInput.DefaultContext.Boost.performed += ToggleModifier;
            }
        }

        private void OnActiveQuestChanged(Quest quest)
        {
            isModifierAvailable = questService.Quests.IndexOf(quest) >= questService.Quests.IndexOf(config.RequiredQuest);
        }

        private void ToggleModifier(InputAction.CallbackContext context)
        {
            if (!isModifierAvailable)
            {
                return;
            }

            isModifierActive = !isModifierActive;
            onIsActiveChanged.Invoke(isModifierActive);
            onSpeedModifierChanged.Invoke(isModifierActive ? config.SpeedModifier : -config.SpeedModifier);
            onFrictionModifierChanged.Invoke(isModifierActive ? config.FrictionModifier : -config.FrictionModifier);
        }
    }
}
