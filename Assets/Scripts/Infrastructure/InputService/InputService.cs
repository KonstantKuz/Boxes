using R3;
using UnityEngine;

namespace Infrastructure.InputService
{
    public class InputService : IInputService
    {
        ReactiveCommand<Vector2> IInputService.LookInput { get; } = new();
        ReactiveCommand<Vector2> IInputService.MoveInput { get; } = new();
        ReactiveCommand<Unit> IInputService.InteractionInput { get; } = new();

        public void Initialize()
        {
            Observable.EveryUpdate(UnityFrameProvider.Update).Subscribe(UpdateInput);
        }

        private void UpdateInput(Unit _)
        {
            Vector2 lookInput = new(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));
            Vector2 moveInput = new(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));

            ((IInputService)this).LookInput.Execute(lookInput);
            ((IInputService)this).MoveInput.Execute(moveInput);

            if (Input.GetKeyDown(KeyCode.F))
            {
                ((IInputService)this).InteractionInput.Execute(Unit.Default);
            }
        }
    }
}
