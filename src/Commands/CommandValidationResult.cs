namespace Bannerlord.RTSCameraLite.Commands
{
    /// <summary>
    /// Outcome of validating a command intent before any engine order is issued.
    /// </summary>
    public sealed class CommandValidationResult
    {
        private CommandValidationResult(bool isValid, string message)
        {
            IsValid = isValid;
            Message = message ?? string.Empty;
        }

        public bool IsValid { get; }

        public string Message { get; }

        public static CommandValidationResult Valid(string message)
        {
            return new CommandValidationResult(true, message);
        }

        public static CommandValidationResult Invalid(string message)
        {
            return new CommandValidationResult(false, message);
        }
    }
}
