using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Layouts;

namespace Infrastructure.InputService.Processors
{
    [System.Serializable]
    [InputControlLayout(displayName = "World Direction Processor")]
    public class MouseToWorldDirectionProcessor : InputProcessor<Vector2>
    {
        [RuntimeInitializeOnLoadMethod]
        private static void Register()
        {
            InputSystem.RegisterProcessor<MouseToWorldDirectionProcessor>();
        }

        public override Vector2 Process(Vector2 value, InputControl control)
        {
            Vector2 screenCenter = new Vector2(Screen.width / 2f, Screen.height / 2f);
            Vector2 direction = value - screenCenter;
            return direction.normalized;
        }
    }
}
