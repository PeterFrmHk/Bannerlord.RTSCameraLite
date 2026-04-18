namespace Bannerlord.RTSCameraLite.Core
{
    /// <summary>
    /// Module identity for logs, diagnostics, and SubModule.xml alignment.
    /// </summary>
    public static class ModConstants
    {
        public const string ModuleId = "Bannerlord.RTSCameraLite";
        public const string DisplayName = "RTS Commander Doctrine";
        public const string LegacyShortName = "RTS Camera Lite";
        public const string Version = "0.1.0-slice7";

        /// <summary>
        /// Pin to your launcher Native module version after local verification (e.g. v1.3.15).
        /// </summary>
        public const string SupportedGameVersion = "VERIFY_LOCAL_BANNERLORD_VERSION";

        /// <summary>
        /// Passed to <c>CommanderModeState.Enable</c> when a supported battle attaches with commander mode on by default (Slice 2).
        /// </summary>
        public const string CommanderShellDefaultEnableReason = "supported mission shell default";
    }
}
