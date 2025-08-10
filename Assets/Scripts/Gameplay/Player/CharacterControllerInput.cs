using CMF;
using Infrastructure.InputService.Abstract;
using Reflex.Attributes;
using UnityEngine;

namespace Gameplay.Player
{
    public class CharacterControllerInput : CharacterInput
    {
        [SerializeField]
        private TurnTowardControllerVelocity turnController;

        private IInputService inputService;
        private Vector2 moveInput;
        private Vector2 lookInput;

        [Inject]
        private void Construct(IInputService inputService)
        {
            this.inputService = inputService;
        }

        public override float GetHorizontalMovementInput()
        {
            return inputService.DefaultContextActions.Move.ReadValue<Vector2>().x;
        }

        public override float GetVerticalMovementInput()
        {
            return inputService.DefaultContextActions.Move.ReadValue<Vector2>().y;
        }

        public override bool IsJumpKeyPressed()
        {
            return inputService.DefaultContextActions.Jump.IsPressed();
        }

        private void Update()
        {
            Vector2 lookDirection = inputService.DefaultContextActions.Aim.ReadValue<Vector2>();
            turnController.SetTargetDirection(lookDirection);
        }
    }
}
