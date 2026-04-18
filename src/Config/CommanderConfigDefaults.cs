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
                EnableCommanderAnchorDebug = true,
                BasicLineMinimumDiscipline = 0.20f,
                LooseMinimumDiscipline = 0.25f,
                ShieldWallMinimumDiscipline = 0.45f,
                SquareMinimumDiscipline = 0.55f,
                CircleMinimumDiscipline = 0.60f,
                AdvancedAdaptiveMinimumDiscipline = 0.75f,
                MinimumShieldRatioForShieldWall = 0.35f,
                MinimumPolearmOrShieldRatioForSquare = 0.45f,
                MinimumMountedRatioForMountedWide = 0.60f,
                MinimumHorseArcherRatioForHorseArcherLoose = 0.45f,
                EnableEligibilityDebug = true
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

        /// <summary>
        /// Older JSON omits Slice 11 floats (deserialize to 0) and EnableEligibilityDebug false.
        /// </summary>
        public static void HarmonizeLegacyEligibilityFields(CommanderConfig config)
        {
            if (config == null)
            {
                return;
            }

            if (!LooksLikeUnsetSlice11Eligibility(config))
            {
                return;
            }

            CommanderConfig d = CreateDefault();
            config.BasicLineMinimumDiscipline = d.BasicLineMinimumDiscipline;
            config.LooseMinimumDiscipline = d.LooseMinimumDiscipline;
            config.ShieldWallMinimumDiscipline = d.ShieldWallMinimumDiscipline;
            config.SquareMinimumDiscipline = d.SquareMinimumDiscipline;
            config.CircleMinimumDiscipline = d.CircleMinimumDiscipline;
            config.AdvancedAdaptiveMinimumDiscipline = d.AdvancedAdaptiveMinimumDiscipline;
            config.MinimumShieldRatioForShieldWall = d.MinimumShieldRatioForShieldWall;
            config.MinimumPolearmOrShieldRatioForSquare = d.MinimumPolearmOrShieldRatioForSquare;
            config.MinimumMountedRatioForMountedWide = d.MinimumMountedRatioForMountedWide;
            config.MinimumHorseArcherRatioForHorseArcherLoose = d.MinimumHorseArcherRatioForHorseArcherLoose;
            config.EnableEligibilityDebug = d.EnableEligibilityDebug;
        }

        private static bool LooksLikeUnsetSlice11Eligibility(CommanderConfig c)
        {
            return !c.EnableEligibilityDebug
                   && c.BasicLineMinimumDiscipline == 0f
                   && c.LooseMinimumDiscipline == 0f
                   && c.ShieldWallMinimumDiscipline == 0f
                   && c.SquareMinimumDiscipline == 0f
                   && c.CircleMinimumDiscipline == 0f
                   && c.AdvancedAdaptiveMinimumDiscipline == 0f
                   && c.MinimumShieldRatioForShieldWall == 0f
                   && c.MinimumPolearmOrShieldRatioForSquare == 0f
                   && c.MinimumMountedRatioForMountedWide == 0f
                   && c.MinimumHorseArcherRatioForHorseArcherLoose == 0f;
        }
    }
}
