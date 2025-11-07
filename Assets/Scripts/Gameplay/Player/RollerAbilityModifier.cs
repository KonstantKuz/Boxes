using System;
using Infrastructure.InputService.Abstract;
using Infrastructure.QuestService;
using Infrastructure.QuestService.Abstract;
using R3;
using Reflex.Attributes;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

namespace Gameplay.Player
{
    public class RollerAbilityModifier : MonoBehaviour
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
        private IQuestService questService;

        private IDisposable questSubscription;
        private bool isModifierAvailable;
        private bool isModifierActive;

        [Inject]
        private void Construct(IInputService inputService, IQuestService questService)
        {
            this.inputService = inputService;
            this.questService = questService;
        }

        private void OnEnable()
        {
            questSubscription = questService.ActiveQuest.Subscribe(OnActiveQuestChanged);
            inputService.DefaultContextActions.Boost.performed += ToggleModifier;
        }

        private void OnDisable()
        {
            questSubscription?.Dispose();
            onSpeedModifierChanged.Invoke(config.SpeedModifier);
            inputService.DefaultContextActions.Boost.performed -= ToggleModifier;
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
