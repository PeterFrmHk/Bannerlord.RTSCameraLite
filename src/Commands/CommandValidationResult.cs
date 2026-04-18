namespace Bannerlord.RTSCameraLite.Commands
{
    /// <summary>Outcome of <see cref="CommandRouter.Validate"/> (Slice 15).</summary>
    public sealed class CommandValidationResult
    {
        private CommandValidationResult(bool isValid, bool isBlocked, string message, CommandIntent intent)
        {
            IsValid = isValid;
            IsBlocked = isBlocked;
            Message = message ?? string.Empty;
            Intent = intent;
        }

        public bool IsValid { get; }

        public bool IsBlocked { get; }

        public string Message { get; }

        public CommandIntent Intent { get; }

        public static CommandValidationResult Valid(string message, CommandIntent intent = null)
        {
            return new CommandValidationResult(true, false, message, intent);
        }

        public static CommandValidationResult Invalid(string message, CommandIntent intent = null)
        {
            return new CommandValidationResult(false, false, message, intent);
        }

        public static CommandValidationResult Blocked(string message, CommandIntent intent = null)
        {
            return new CommandValidationResult(false, true, message, intent);
        }
    }
}
