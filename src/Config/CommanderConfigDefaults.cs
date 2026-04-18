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
                ConfigFileVersion = CommanderConfigSchema.CurrentConfigVersion,
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
                MoraleWeight = 0.15f,
                TrainingWeight = 0.20f,
                EquipmentWeight = 0.20f,
                CommanderWeight = 0.20f,
                CohesionWeight = 0.10f,
                RankWeight = 0.15f,
                CasualtyShockPenaltyWeight = 0.20f,
                EnableDoctrineDebug = true,
                DoctrineScanIntervalSeconds = 3.0f,
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
                EnableEligibilityDebug = true,
                CommanderRallyRadius = 25.0f,
                CommanderAbsorptionRadius = 10.0f,
                FormationSlotRadius = 2.0f,
                CohesionBreakRadius = 35.0f,
                SlotReassignmentCooldownSeconds = 3.0f,
                RallyScanIntervalSeconds = 3.0f,
                EnableRallyAbsorptionDebug = true,
                CavalryLateralSpacing = 4.5f,
                CavalryDepthSpacing = 6.0f,
                HorseArcherLateralSpacing = 7.0f,
                HorseArcherDepthSpacing = 8.0f,
                CavalryReleaseLockDistance = 12.0f,
                CavalryReformDistanceFromAttackedFormation = 30.0f,
                CavalryReformCooldownSeconds = 6.0f,
                CavalryMinimumEnemyDistanceToReform = 20.0f,
                CavalryImpactEnemyDistance = 7.5f,
                CavalryImpactSpeedDropThreshold = 0.35f,
                CavalryImpactAgentRatio = 0.25f,
                EnableCavalryDoctrineDebug = true,
                AllowCavalryReformWithoutCommander = false,
                EnableNativeOrderExecution = false,
                AllowNativeAdvanceOrMove = true,
                AllowNativeCharge = true,
                AllowNativeHold = true,
                AllowNativeReform = true,
                AllowNativeFollowCommander = true,
                AllowNativeStop = true,
                EnableNativeOrderDebug = true,
                EnableCommandRouter = true,
                EnableCommandValidationDebug = true,
                AllowBasicChargeWithoutAdvancedDoctrine = true,
                AllowNoCommanderBasicHold = true,
                AllowNoCommanderBasicFollow = true,
                BlockAdvancedCommandsWithoutCommander = true,
                EnableNativePrimitiveOrderExecution = false,
                EnableNativeCavalryChargeSequence = false,
                CavalryUseNativeForwardBeforeCharge = true,
                CavalryUseNativeChargeCommand = true,
                CavalryForwardToChargeDistance = 25.0f,
                EnableCavalrySequenceDebug = true,
                EnableCommandMarkers = true,
                EnableFallbackTextMarkers = true,
                DefaultMarkerLifetimeSeconds = 2.5f,
                ChargeMarkerLifetimeSeconds = 3.0f,
                ReformMarkerLifetimeSeconds = 3.0f,
                MarkerRefreshThrottleSeconds = 0.25f,
                EnableDiagnostics = true,
                ShowDiagnosticsInCommanderModeOnly = true,
                DiagnosticsToggleKey = "F9",
                DiagnosticsRefreshIntervalSeconds = 1.0f,
                IncludeDoctrineScores = true,
                IncludeEligibility = true,
                IncludeRallyAbsorption = true,
                IncludeCavalrySequence = true,
                IncludeNativeOrderStatus = true,
                CommandValidationDebugLogIntervalSeconds = 0.4f
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

        /// <summary>
        /// Older JSON omits Slice 10 doctrine keys — floats deserialize to 0 and booleans to false.
        /// </summary>
        public static void HarmonizeLegacyDoctrineFields(CommanderConfig config)
        {
            if (config == null)
            {
                return;
            }

            if (!LooksLikeUnsetSlice10Doctrine(config))
            {
                return;
            }

            CommanderConfig d = CreateDefault();
            config.MoraleWeight = d.MoraleWeight;
            config.TrainingWeight = d.TrainingWeight;
            config.EquipmentWeight = d.EquipmentWeight;
            config.CommanderWeight = d.CommanderWeight;
            config.CohesionWeight = d.CohesionWeight;
            config.RankWeight = d.RankWeight;
            config.CasualtyShockPenaltyWeight = d.CasualtyShockPenaltyWeight;
            config.EnableDoctrineDebug = d.EnableDoctrineDebug;
            config.DoctrineScanIntervalSeconds = d.DoctrineScanIntervalSeconds;
        }

        private static bool LooksLikeUnsetSlice10Doctrine(CommanderConfig c)
        {
            return c.MoraleWeight == 0f
                   && c.TrainingWeight == 0f
                   && c.EquipmentWeight == 0f
                   && c.CommanderWeight == 0f
                   && c.CohesionWeight == 0f
                   && c.RankWeight == 0f
                   && c.CasualtyShockPenaltyWeight == 0f
                   && !c.EnableDoctrineDebug
                   && c.DoctrineScanIntervalSeconds == 0f;
        }

        /// <summary>
        /// Older JSON omits Slice 12 rally keys — floats deserialize to 0 and booleans to false.
        /// </summary>
        public static void HarmonizeLegacyRallyFields(CommanderConfig config)
        {
            if (config == null)
            {
                return;
            }

            if (!LooksLikeUnsetSlice12Rally(config))
            {
                return;
            }

            CommanderConfig d = CreateDefault();
            config.CommanderRallyRadius = d.CommanderRallyRadius;
            config.CommanderAbsorptionRadius = d.CommanderAbsorptionRadius;
            config.FormationSlotRadius = d.FormationSlotRadius;
            config.CohesionBreakRadius = d.CohesionBreakRadius;
            config.SlotReassignmentCooldownSeconds = d.SlotReassignmentCooldownSeconds;
            config.RallyScanIntervalSeconds = d.RallyScanIntervalSeconds;
            config.EnableRallyAbsorptionDebug = d.EnableRallyAbsorptionDebug;
        }

        private static bool LooksLikeUnsetSlice12Rally(CommanderConfig c)
        {
            return c.CommanderRallyRadius == 0f
                   && c.CommanderAbsorptionRadius == 0f
                   && c.FormationSlotRadius == 0f
                   && c.CohesionBreakRadius == 0f
                   && c.SlotReassignmentCooldownSeconds == 0f
                   && c.RallyScanIntervalSeconds == 0f
                   && !c.EnableRallyAbsorptionDebug;
        }

        /// <summary>
        /// Older JSON omits Slice 13 cavalry keys — floats deserialize to 0 and booleans to false.
        /// </summary>
        public static void HarmonizeLegacyCavalryDoctrineFields(CommanderConfig config)
        {
            if (config == null)
            {
                return;
            }

            if (!LooksLikeUnsetSlice13Cavalry(config))
            {
                return;
            }

            CommanderConfig d = CreateDefault();
            config.CavalryLateralSpacing = d.CavalryLateralSpacing;
            config.CavalryDepthSpacing = d.CavalryDepthSpacing;
            config.HorseArcherLateralSpacing = d.HorseArcherLateralSpacing;
            config.HorseArcherDepthSpacing = d.HorseArcherDepthSpacing;
            config.CavalryReleaseLockDistance = d.CavalryReleaseLockDistance;
            config.CavalryReformDistanceFromAttackedFormation = d.CavalryReformDistanceFromAttackedFormation;
            config.CavalryReformCooldownSeconds = d.CavalryReformCooldownSeconds;
            config.CavalryMinimumEnemyDistanceToReform = d.CavalryMinimumEnemyDistanceToReform;
            config.CavalryImpactEnemyDistance = d.CavalryImpactEnemyDistance;
            config.CavalryImpactSpeedDropThreshold = d.CavalryImpactSpeedDropThreshold;
            config.CavalryImpactAgentRatio = d.CavalryImpactAgentRatio;
            config.EnableCavalryDoctrineDebug = d.EnableCavalryDoctrineDebug;
            config.AllowCavalryReformWithoutCommander = d.AllowCavalryReformWithoutCommander;
        }

        private static bool LooksLikeUnsetSlice13Cavalry(CommanderConfig c)
        {
            return c.CavalryLateralSpacing == 0f
                   && c.CavalryDepthSpacing == 0f
                   && c.HorseArcherLateralSpacing == 0f
                   && c.HorseArcherDepthSpacing == 0f
                   && c.CavalryReleaseLockDistance == 0f
                   && c.CavalryReformDistanceFromAttackedFormation == 0f
                   && c.CavalryReformCooldownSeconds == 0f
                   && c.CavalryMinimumEnemyDistanceToReform == 0f
                   && c.CavalryImpactEnemyDistance == 0f
                   && c.CavalryImpactSpeedDropThreshold == 0f
                   && c.CavalryImpactAgentRatio == 0f
                   && !c.EnableCavalryDoctrineDebug
                   && !c.AllowCavalryReformWithoutCommander;
        }

        /// <summary>
        /// Older JSON omits Slice 15 keys — booleans default false, floats to 0.
        /// </summary>
        public static void HarmonizeLegacyCommandRouterFields(CommanderConfig config)
        {
            if (config == null)
            {
                return;
            }

            if (!LooksLikeUnsetSlice15CommandRouter(config))
            {
                return;
            }

            CommanderConfig d = CreateDefault();
            config.EnableCommandRouter = d.EnableCommandRouter;
            config.EnableCommandValidationDebug = d.EnableCommandValidationDebug;
            config.AllowBasicChargeWithoutAdvancedDoctrine = d.AllowBasicChargeWithoutAdvancedDoctrine;
            config.AllowNoCommanderBasicHold = d.AllowNoCommanderBasicHold;
            config.AllowNoCommanderBasicFollow = d.AllowNoCommanderBasicFollow;
            config.BlockAdvancedCommandsWithoutCommander = d.BlockAdvancedCommandsWithoutCommander;
            config.EnableNativePrimitiveOrderExecution = d.EnableNativePrimitiveOrderExecution;
            config.CommandValidationDebugLogIntervalSeconds = d.CommandValidationDebugLogIntervalSeconds;
        }

        private static bool LooksLikeUnsetSlice15CommandRouter(CommanderConfig c)
        {
            return !c.EnableCommandRouter
                   && !c.EnableCommandValidationDebug
                   && !c.AllowBasicChargeWithoutAdvancedDoctrine
                   && !c.AllowNoCommanderBasicHold
                   && !c.AllowNoCommanderBasicFollow
                   && !c.BlockAdvancedCommandsWithoutCommander
                   && !c.EnableNativePrimitiveOrderExecution
                   && c.CommandValidationDebugLogIntervalSeconds == 0f;
        }
    }
}
