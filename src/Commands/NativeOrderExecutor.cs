using System;

namespace Bannerlord.RTSCameraLite.Commands
{
    /// <summary>
    /// Legacy RTS debug path (not compiled in <c>Bannerlord.RTSCameraLite.csproj</c>). Native issuance is
    /// <see cref="NativeOrderPrimitiveExecutor"/> only (Slice 14).
    /// </summary>
    internal sealed class NativeOrderExecutor
    {
        public CommandExecutionResult Execute(CommandIntent intent, CommandContext context)
        {
            try
            {
                CommandType t = intent?.Type ?? CommandType.None;
                return CommandExecutionResult.Failure(
                    t,
                    "NativeOrderExecutor is retired; use NativeOrderPrimitiveExecutor (Slice 14).");
            }
            catch (Exception ex)
            {
                CommandType t = intent?.Type ?? CommandType.None;
                return CommandExecutionResult.Failure(t, $"Order execution error: {ex.Message}");
            }
        }
    }
}
