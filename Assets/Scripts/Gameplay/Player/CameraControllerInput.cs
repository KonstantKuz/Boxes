using CMF;
using Infrastructure.InputService.Abstract;
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

        [Inject]
        private void Construct(IInputService inputService)
        {
            this.inputService = inputService;
        }

        public override float GetHorizontalCameraInput()
        {
            //Get raw mouse input;
            float input = inputService.DefaultContextActions.Aim.ReadValue<Vector2>().x;

            //Since raw mouse input is already time-based, we need to correct for this before passing the input to the camera controller;
            if(Time.timeScale > 0f && Time.deltaTime > 0f)
            {
                input /= Time.deltaTime;
                input *= Time.timeScale;
            }
            else
                input = 0f;

            //Apply mouse sensitivity;
            input *= mouseInputMultiplier;

            //Invert input;
            if(invertHorizontalInput)
                input *= -1f;

            return input;
        }

        public override float GetVerticalCameraInput()
        {
            //Get raw mouse input;
            float input = -inputService.DefaultContextActions.Aim.ReadValue<Vector2>().y;

            //Since raw mouse input is already time-based, we need to correct for this before passing the input to the camera controller;
            if(Time.timeScale > 0f && Time.deltaTime > 0f)
            {
                input /= Time.deltaTime;
                input *= Time.timeScale;
            }
            else
                input = 0f;

            //Apply mouse sensitivity;
            input *= mouseInputMultiplier;

            //Invert input;
            if(invertVerticalInput)
                input *= -1f;

            return input;
        }
    }
}
