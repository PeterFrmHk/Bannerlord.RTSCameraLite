using Bannerlord.RTSCameraLite.Core;

namespace Bannerlord.RTSCameraLite.Mission
{
    /// <summary>
    /// Tracks whether RTS Commander Mode is active for the current mission shell (Slice 2: no camera/commands yet).
    /// </summary>
    public sealed class CommanderModeState
    {
        public bool IsEnabled { get; private set; }

        /// <summary>
        /// True when the shell opened with commander mode enabled (<see cref="ModConstants.CommanderShellDefaultEnableReason"/>).
        /// </summary>
        public bool StartsEnabled { get; private set; }

        public int ToggleCount { get; private set; }

        public string LastToggleReason { get; private set; } = string.Empty;

        public void Enable(string reason)
        {
            LastToggleReason = reason ?? string.Empty;
            if (IsEnabled)
            {
                return;
            }

            IsEnabled = true;
            if (string.Equals(
                    reason,
                    ModConstants.CommanderShellDefaultEnableReason,
                    System.StringComparison.Ordinal))
            {
                StartsEnabled = true;
            }
        }

        public void Disable(string reason)
        {
            LastToggleReason = reason ?? string.Empty;
            if (!IsEnabled)
            {
                return;
            }

            IsEnabled = false;
        }

        public void Toggle(string reason)
        {
            LastToggleReason = reason ?? string.Empty;
            if (IsEnabled)
            {
                IsEnabled = false;
            }
            else
            {
                IsEnabled = true;
            }

            ToggleCount++;
        }

        public void ForceDisabled(string reason)
        {
            LastToggleReason = reason ?? string.Empty;
            IsEnabled = false;
        }
    }
}
