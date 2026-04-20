using Bannerlord.RTSCameraLite.Config;

namespace Bannerlord.RTSCameraLite.Commands
{
    /// <summary>
    /// Pure TW-3 execution gate for native ground move issuance. Defaults fail closed.
    /// </summary>
    public static class GroundMoveExecutionGate
    {
        public static bool CanExecute(CommanderConfig config, bool nativeExecutorAvailable, out string reason)
        {
            reason = string.Empty;
            if (config == null)
            {
                reason = "config unavailable";
                return false;
            }

            if (!config.EnableMissionRuntimeHooks)
            {
                reason = "mission runtime hooks disabled";
                return false;
            }

            if (!config.EnableFormationSelection)
            {
                reason = "formation selection disabled";
                return false;
            }

            if (!config.EnableGroundCommandPreview)
            {
                reason = "ground command preview disabled";
                return false;
            }

            if (!config.EnableGroundMoveExecution)
            {
                reason = "ground move execution disabled";
                return false;
            }

            if (!config.EnableCommandRouter)
            {
                reason = "command router disabled";
                return false;
            }

            if (!config.EnableNativePrimitiveOrderExecution)
            {
                reason = "native primitive order execution disabled";
                return false;
            }

            if (!config.EnableNativeOrderExecution)
            {
                reason = "native order execution disabled";
                return false;
            }

            if (!nativeExecutorAvailable)
            {
                reason = "native order executor unavailable";
                return false;
            }

            return true;
        }
    }
}
