namespace Bannerlord.RTSCameraLite.Config
{
    /// <summary>
    /// Canonical defaults when no file exists, JSON is invalid, or individual keys fail to parse.
    /// </summary>
    public static class CommanderConfigDefaults
    {
        public const string ConfigFileName = "commander_config.json";

        /// <summary>Relative to the module root (e.g. <c>Modules/Bannerlord.RTSCameraLite/config/</c>).</summary>
        public const string ConfigRelativeDirectory = "config";

        public static string RelativeConfigPath => System.IO.Path.Combine(ConfigRelativeDirectory, ConfigFileName);

        public static CommanderConfig CreateDefault()
        {
            return new CommanderConfig
            {
                StartBattlesInCommanderMode = true,
                ModeActivationKey = "Backspace",
                OverrideNativeBackspaceOrders = true,
                EnableDebugFallbackToggle = true,
                DebugFallbackToggleKey = "F10",
                MoveForwardKey = "W",
                MoveBackKey = "S",
                MoveLeftKey = "A",
                MoveRightKey = "D",
                RotateLeftKey = "Q",
                RotateRightKey = "E",
                FastMoveKey = "LeftShift",
                ZoomInKey = "R",
                ZoomOutKey = "F",
                MoveSpeed = 12.0f,
                FastMoveMultiplier = 2.5f,
                RotationSpeedDegrees = 90.0f,
                ZoomSpeed = 3.0f,
                DefaultHeight = 18.0f,
                MinHeight = 6.0f,
                MaxHeight = 60.0f,
                DefaultPitch = 60.0f
            };
        }
    }
}
