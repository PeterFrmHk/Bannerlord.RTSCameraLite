using TaleWorlds.Engine;
using TaleWorlds.InputSystem;

namespace Bannerlord.RTSCameraLite.Input
{
    /// <summary>
    /// Polls TaleWorlds input for RTS camera (read-only; does not consume native input).
    /// </summary>
    internal static class RTSCameraInput
    {
        public static InputSnapshot Read(IInputContext input)
        {
            if (input == null)
            {
                return new InputSnapshot(false, false, false, false, false, false, false, 0f);
            }

            bool forward = input.IsKeyDown(InputKey.W);
            bool back = input.IsKeyDown(InputKey.S);
            bool left = input.IsKeyDown(InputKey.A);
            bool right = input.IsKeyDown(InputKey.D);
            bool rotateLeft = input.IsKeyDown(InputKey.Q);
            bool rotateRight = input.IsKeyDown(InputKey.E);
            bool fastMove = input.IsKeyDown(InputKey.LeftShift) || input.IsKeyDown(InputKey.RightShift);

            float zoom = 0f;
            if (input.IsKeyPressed(InputKey.MouseScrollUp))
            {
                zoom += 1f;
            }

            if (input.IsKeyPressed(InputKey.MouseScrollDown))
            {
                zoom -= 1f;
            }

            // Fallback if wheel bindings do not fire on a given machine / layout.
            if (input.IsKeyDown(InputKey.R))
            {
                zoom += 0.35f;
            }

            if (input.IsKeyDown(InputKey.F))
            {
                zoom -= 0.35f;
            }

            return new InputSnapshot(
                forward,
                back,
                left,
                right,
                rotateLeft,
                rotateRight,
                fastMove,
                zoom);
        }
    }
}
