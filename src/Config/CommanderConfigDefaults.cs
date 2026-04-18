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
                AllowNativeOrdersWhenCommanderModeDisabled = true,
                EnableInputOwnershipGuard = true,
                SuppressNativeMovementInCommanderMode = false,
                SuppressNativeCombatInCommanderMode = false,
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
                DefaultPitch = 60.0f,
                RequireHeroCommanderForAdvancedFormations = true,
                AllowCaptainCommander = true,
                AllowSergeantFallback = false,
                AllowHighestTierFallback = false,
                NoCommanderAllowsBasicMobOrders = true,
                MinimumCommandAuthorityScore = 0.25f,
                DefaultCommanderBackOffset = 6.0f,
                ShieldWallCommanderBackOffset = 8.0f,
                ArcherCommanderBackOffset = 7.0f,
                CavalryCommanderBackOffset = 10.0f,
                SkirmisherCommanderBackOffset = 9.0f,
                AnchorAllowedRadius = 4.0f,
                EnableCommanderAnchorDebug = true
            };
        }

        /// <summary>
        /// Older JSON files omit Slice 8 keys; System.Text.Json leaves bools false and floats 0. Restore detection defaults when that pattern matches.
        /// </summary>
        public static void HarmonizeLegacyDetectionFields(CommanderConfig config)
        {
            if (config == null)
            {
                return;
            }

            if (!LooksLikeUnsetSlice8Detection(config))
            {
                return;
            }

            CommanderConfig d = CreateDefault();
            config.RequireHeroCommanderForAdvancedFormations = d.RequireHeroCommanderForAdvancedFormations;
            config.AllowCaptainCommander = d.AllowCaptainCommander;
            config.AllowSergeantFallback = d.AllowSergeantFallback;
            config.AllowHighestTierFallback = d.AllowHighestTierFallback;
            config.NoCommanderAllowsBasicMobOrders = d.NoCommanderAllowsBasicMobOrders;
            config.MinimumCommandAuthorityScore = d.MinimumCommandAuthorityScore;
        }

        private static bool LooksLikeUnsetSlice8Detection(CommanderConfig c)
        {
            return !c.RequireHeroCommanderForAdvancedFormations
                   && !c.AllowCaptainCommander
                   && !c.AllowSergeantFallback
                   && !c.AllowHighestTierFallback
                   && !c.NoCommanderAllowsBasicMobOrders
                   && c.MinimumCommandAuthorityScore == 0f;
        }
    }
}
