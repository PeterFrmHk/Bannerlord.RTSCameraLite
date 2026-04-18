using Bannerlord.RTSCameraLite.Config;
using TaleWorlds.InputSystem;

namespace Bannerlord.RTSCameraLite.Input
{
    /// <summary>
    /// Polls TaleWorlds input for RTS camera (read-only; does not consume native input).
    /// All RTS-specific reads stay here. Key names from JSON are parsed when config loads
    /// (<see cref="ConfigService"/>); bindings carry resolved <see cref="InputKey"/> values.
    /// </summary>
    internal static class RTSCameraInput
    {
        private static readonly InputSnapshot EmptySnapshot =
            new InputSnapshot(false, false, false, false, false, false, false, 0f, false, false, false);

        public static InputSnapshot Read(
            IInputContext input,
            InputOwnershipState ownership,
            ConfigService.CameraKeyBindings keys)
        {
            if (ownership == null || !ownership.RtsModeOwnsCameraInput)
            {
                return EmptySnapshot;
            }

            return ReadInternal(input, keys);
        }

        private static InputSnapshot ReadInternal(IInputContext input, ConfigService.CameraKeyBindings keys)
        {
            if (input == null)
            {
                return EmptySnapshot;
            }

            bool forward = IsKeyDownSafe(input, keys.MoveForward);
            bool back = IsKeyDownSafe(input, keys.MoveBack);
            bool left = IsKeyDownSafe(input, keys.MoveLeft);
            bool right = IsKeyDownSafe(input, keys.MoveRight);
            bool rotateLeft = IsKeyDownSafe(input, keys.RotateLeft);
            bool rotateRight = IsKeyDownSafe(input, keys.RotateRight);
            bool fastMove = IsKeyDownSafe(input, keys.FastMove);

            float zoom = 0f;
            if (input.IsKeyPressed(InputKey.MouseScrollUp))
            {
                zoom += 1f;
            }

            if (input.IsKeyPressed(InputKey.MouseScrollDown))
            {
                zoom -= 1f;
            }

            if (IsKeyDownSafe(input, keys.ZoomIn))
            {
                zoom += 0.35f;
            }

            if (IsKeyDownSafe(input, keys.ZoomOut))
            {
                zoom -= 0.35f;
            }

            bool nextFormation = IsKeyPressedSafe(input, keys.NextFormation);
            bool previousFormation = IsKeyPressedSafe(input, keys.PreviousFormation);
            bool focusFormation = IsKeyPressedSafe(input, keys.FocusSelectedFormation);

            return new InputSnapshot(
                forward,
                back,
                left,
                right,
                rotateLeft,
                rotateRight,
                fastMove,
                zoom,
                nextFormation,
                previousFormation,
                focusFormation);
        }

        private static bool IsKeyDownSafe(IInputContext input, InputKey key)
        {
            try
            {
                return input.IsKeyDown(key);
            }
            catch
            {
                return false;
            }
        }

        private static bool IsKeyPressedSafe(IInputContext input, InputKey key)
        {
            try
            {
                return input.IsKeyPressed(key);
            }
            catch
            {
                return false;
            }
        }
    }
}
