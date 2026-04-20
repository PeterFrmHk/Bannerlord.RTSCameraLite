using TaleWorlds.InputSystem;

namespace Bannerlord.RTSCameraLite.Input
{
    /// <summary>
    /// TW command-layer gesture parser. TW-2 uses a temporary G-key ground preview gesture to avoid native right-click order conflicts.
    /// </summary>
    public sealed class CommandGestureParser
    {
        public GroundCommandPreviewRequest TryReadGroundCommandPreview(
            IInputContext input,
            bool runtimeHooksEnabled,
            bool formationSelectionEnabled,
            bool groundPreviewEnabled,
            bool commanderModeEnabled)
        {
            if (!runtimeHooksEnabled || !formationSelectionEnabled || !groundPreviewEnabled || !commanderModeEnabled)
            {
                return GroundCommandPreviewRequest.NotRequested("Ground command preview disabled.");
            }

            if (input == null)
            {
                return GroundCommandPreviewRequest.NotRequested("Input unavailable.");
            }

            try
            {
                if (input.IsKeyReleased(InputKey.G))
                {
                    bool executeRequested = input.IsKeyDown(InputKey.LeftShift) || input.IsKeyDown(InputKey.RightShift);
                    return GroundCommandPreviewRequest.CreateRequested(executeRequested ? "Shift+G" : "G", executeRequested);
                }
            }
            catch
            {
                return GroundCommandPreviewRequest.NotRequested("Ground preview input unavailable.");
            }

            return GroundCommandPreviewRequest.NotRequested("No ground preview gesture.");
        }
    }

    public readonly struct GroundCommandPreviewRequest
    {
        private GroundCommandPreviewRequest(bool requested, bool executeRequested, string gesture, string message)
        {
            Requested = requested;
            ExecuteRequested = executeRequested;
            Gesture = gesture ?? string.Empty;
            Message = message ?? string.Empty;
        }

        public bool Requested { get; }

        public bool ExecuteRequested { get; }

        public string Gesture { get; }

        public string Message { get; }

        public static GroundCommandPreviewRequest CreateRequested(string gesture, bool executeRequested)
        {
            return new GroundCommandPreviewRequest(true, executeRequested, gesture, string.Empty);
        }

        public static GroundCommandPreviewRequest NotRequested(string message)
        {
            return new GroundCommandPreviewRequest(false, false, string.Empty, message);
        }
    }
}
