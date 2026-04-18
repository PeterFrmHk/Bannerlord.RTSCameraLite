using TaleWorlds.Library;

namespace Bannerlord.RTSCameraLite.Commands
{
    /// <summary>
    /// Outcome of attempting to issue a native Bannerlord order (slice 12).
    /// </summary>
    public sealed class CommandExecutionResult
    {
        private CommandExecutionResult(bool executed, CommandType type, string message, Vec3? markerWorldPosition)
        {
            Executed = executed;
            Type = type;
            Message = message ?? string.Empty;
            MarkerWorldPosition = markerWorldPosition;
        }

        public bool Executed { get; }

        public CommandType Type { get; }

        public string Message { get; }

        /// <summary>
        /// When set, callers may show a short-lived world marker (slice 13), e.g. successful move destination.
        /// </summary>
        public Vec3? MarkerWorldPosition { get; }

        public static CommandExecutionResult Success(CommandType type, string message, Vec3? markerWorldPosition = null)
        {
            return new CommandExecutionResult(true, type, message, markerWorldPosition);
        }

        public static CommandExecutionResult Failure(CommandType type, string message)
        {
            return new CommandExecutionResult(false, type, message, null);
        }
    }
}
