using TaleWorlds.InputSystem;

namespace Bannerlord.RTSCameraLite.Input
{
    /// <summary>
    /// Centralized key reads for RTS Commander Mode (Slice 2). Do not poll <see cref="IInputContext"/> for these keys elsewhere.
    /// </summary>
    public sealed class CommanderInputReader
    {
        public bool TryConsumeCommanderModeToggle(IInputContext input)
        {
            if (input == null)
            {
                return false;
            }

            try
            {
                return input.IsKeyReleased(InputKey.BackSpace);
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// DEBUG / DEVELOPMENT ONLY — emergency toggle if Backspace is unavailable.
        /// </summary>
        public bool TryConsumeEmergencyDebugCommanderToggle(IInputContext input)
        {
            if (input == null)
            {
                return false;
            }

            try
            {
                return input.IsKeyReleased(InputKey.F10);
            }
            catch
            {
                return false;
            }
        }
    }
}
