using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Layouts;

namespace Infrastructure.InputService.Processors
{
    [System.Serializable]
    [InputControlLayout(displayName = "World Direction Processor")]
    public class MouseToWorldDirectionProcessor : InputProcessor<Vector2>
    {
        private static Camera Camera;
        private static Transform RelativeTarget;

        [RuntimeInitializeOnLoadMethod]
        private static void Register()
        {
            InputSystem.RegisterProcessor<MouseToWorldDirectionProcessor>();
        }

        public static void SetParams(Camera camera, Transform relativeTarget)
        {
            Camera = camera;
            RelativeTarget = relativeTarget;
        }

        public override Vector2 Process(Vector2 value, InputControl control)
        {
            if (Camera == null || RelativeTarget == null)
            {
                return Vector2.zero;
            }

            Vector2 targetScreenPoint = Camera.WorldToScreenPoint(RelativeTarget.position);
            Vector2 direction = value - targetScreenPoint;
            return direction.normalized;
        }
    }
}
