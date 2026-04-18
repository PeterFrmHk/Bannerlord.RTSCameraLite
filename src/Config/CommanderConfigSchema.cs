using System;
using System.Collections.Generic;
using System.Reflection;

namespace Bannerlord.RTSCameraLite.Config
{
    /// <summary>
    /// Schema metadata for <see cref="CommanderConfig"/> (Slice 23): version stamp and documentation-oriented field lists.
    /// </summary>
    public static class CommanderConfigSchema
    {
        /// <summary>Increment when migration steps must run for older on-disk files.</summary>
        public const int CurrentConfigVersion = 1;

        /// <summary>Human-readable schema label for support and generated notes.</summary>
        public const string SchemaLabel = "slice-23-v1";

        private static readonly Lazy<HashSet<string>> KnownRootPropertyNamesLazy = new Lazy<HashSet<string>>(BuildKnownRootPropertyNames);

        /// <summary>JSON root property names accepted by <see cref="CommanderConfig"/> (case-insensitive).</summary>
        public static HashSet<string> KnownRootPropertyNames => KnownRootPropertyNamesLazy.Value;

        /// <summary>Major config groups for documentation and simple tooling.</summary>
        public static IReadOnlyList<(string Title, string[] Fields)> DocumentationGroups { get; } =
            new List<(string, string[])>
            {
                ("Mode and policy", new[]
                {
                    nameof(CommanderConfig.ConfigFileVersion),
                    nameof(CommanderConfig.EnableMissionRuntimeHooks),
                    nameof(CommanderConfig.EnableHarmonyPatches),
                    nameof(CommanderConfig.EnableHarmonyDiagnostics),
                    nameof(CommanderConfig.StartBattlesInCommanderMode),
                    nameof(CommanderConfig.ModeActivationKey),
                    nameof(CommanderConfig.OverrideNativeBackspaceOrders),
                    nameof(CommanderConfig.AllowNativeOrdersWhenCommanderModeDisabled),
                    nameof(CommanderConfig.EnableInputOwnershipGuard),
                    nameof(CommanderConfig.SuppressNativeMovementInCommanderMode),
                    nameof(CommanderConfig.SuppressNativeCombatInCommanderMode),
                    nameof(CommanderConfig.EnableDebugFallbackToggle),
                    nameof(CommanderConfig.DebugFallbackToggleKey)
                }),
                ("Camera movement keys", new[]
                {
                    nameof(CommanderConfig.MoveForwardKey),
                    nameof(CommanderConfig.MoveBackKey),
                    nameof(CommanderConfig.MoveLeftKey),
                    nameof(CommanderConfig.MoveRightKey),
                    nameof(CommanderConfig.RotateLeftKey),
                    nameof(CommanderConfig.RotateRightKey),
                    nameof(CommanderConfig.FastMoveKey),
                    nameof(CommanderConfig.ZoomInKey),
                    nameof(CommanderConfig.ZoomOutKey)
                }),
                ("Camera tuning", new[]
                {
                    nameof(CommanderConfig.MoveSpeed),
                    nameof(CommanderConfig.FastMoveMultiplier),
                    nameof(CommanderConfig.RotationSpeedDegrees),
                    nameof(CommanderConfig.ZoomSpeed),
                    nameof(CommanderConfig.DefaultHeight),
                    nameof(CommanderConfig.MinHeight),
                    nameof(CommanderConfig.MaxHeight),
                    nameof(CommanderConfig.DefaultPitch)
                }),
                ("Commander presence", new[]
                {
                    nameof(CommanderConfig.RequireHeroCommanderForAdvancedFormations),
                    nameof(CommanderConfig.AllowCaptainCommander),
                    nameof(CommanderConfig.AllowSergeantFallback),
                    nameof(CommanderConfig.AllowHighestTierFallback),
                    nameof(CommanderConfig.NoCommanderAllowsBasicMobOrders),
                    nameof(CommanderConfig.MinimumCommandAuthorityScore)
                }),
                ("Commander anchor", new[]
                {
                    nameof(CommanderConfig.DefaultCommanderBackOffset),
                    nameof(CommanderConfig.ShieldWallCommanderBackOffset),
                    nameof(CommanderConfig.ArcherCommanderBackOffset),
                    nameof(CommanderConfig.CavalryCommanderBackOffset),
                    nameof(CommanderConfig.SkirmisherCommanderBackOffset),
                    nameof(CommanderConfig.AnchorAllowedRadius),
                    nameof(CommanderConfig.EnableCommanderAnchorDebug)
                }),
                ("Doctrine scoring", new[]
                {
                    nameof(CommanderConfig.MoraleWeight),
                    nameof(CommanderConfig.TrainingWeight),
                    nameof(CommanderConfig.EquipmentWeight),
                    nameof(CommanderConfig.CommanderWeight),
                    nameof(CommanderConfig.CohesionWeight),
                    nameof(CommanderConfig.RankWeight),
                    nameof(CommanderConfig.CasualtyShockPenaltyWeight),
                    nameof(CommanderConfig.EnableDoctrineDebug),
                    nameof(CommanderConfig.DoctrineScanIntervalSeconds)
                }),
                ("Formation eligibility", new[]
                {
                    nameof(CommanderConfig.BasicLineMinimumDiscipline),
                    nameof(CommanderConfig.LooseMinimumDiscipline),
                    nameof(CommanderConfig.ShieldWallMinimumDiscipline),
                    nameof(CommanderConfig.SquareMinimumDiscipline),
                    nameof(CommanderConfig.CircleMinimumDiscipline),
                    nameof(CommanderConfig.AdvancedAdaptiveMinimumDiscipline),
                    nameof(CommanderConfig.MinimumShieldRatioForShieldWall),
                    nameof(CommanderConfig.MinimumPolearmOrShieldRatioForSquare),
                    nameof(CommanderConfig.MinimumMountedRatioForMountedWide),
                    nameof(CommanderConfig.MinimumHorseArcherRatioForHorseArcherLoose),
                    nameof(CommanderConfig.EnableEligibilityDebug)
                }),
                ("Rally and absorption", new[]
                {
                    nameof(CommanderConfig.CommanderRallyRadius),
                    nameof(CommanderConfig.CommanderAbsorptionRadius),
                    nameof(CommanderConfig.FormationSlotRadius),
                    nameof(CommanderConfig.CohesionBreakRadius),
                    nameof(CommanderConfig.SlotReassignmentCooldownSeconds),
                    nameof(CommanderConfig.RallyScanIntervalSeconds),
                    nameof(CommanderConfig.EnableRallyAbsorptionDebug)
                }),
                ("Cavalry doctrine", new[]
                {
                    nameof(CommanderConfig.CavalryLateralSpacing),
                    nameof(CommanderConfig.CavalryDepthSpacing),
                    nameof(CommanderConfig.HorseArcherLateralSpacing),
                    nameof(CommanderConfig.HorseArcherDepthSpacing),
                    nameof(CommanderConfig.CavalryReleaseLockDistance),
                    nameof(CommanderConfig.CavalryReformDistanceFromAttackedFormation),
                    nameof(CommanderConfig.CavalryReformCooldownSeconds),
                    nameof(CommanderConfig.CavalryMinimumEnemyDistanceToReform),
                    nameof(CommanderConfig.CavalryImpactEnemyDistance),
                    nameof(CommanderConfig.CavalryImpactSpeedDropThreshold),
                    nameof(CommanderConfig.CavalryImpactAgentRatio),
                    nameof(CommanderConfig.EnableCavalryDoctrineDebug),
                    nameof(CommanderConfig.AllowCavalryReformWithoutCommander)
                }),
                ("Native order execution (opt-in)", new[]
                {
                    nameof(CommanderConfig.EnableNativeOrderExecution),
                    nameof(CommanderConfig.AllowNativeAdvanceOrMove),
                    nameof(CommanderConfig.AllowNativeCharge),
                    nameof(CommanderConfig.AllowNativeHold),
                    nameof(CommanderConfig.AllowNativeReform),
                    nameof(CommanderConfig.AllowNativeFollowCommander),
                    nameof(CommanderConfig.AllowNativeStop),
                    nameof(CommanderConfig.EnableNativeOrderDebug)
                }),
                ("Command router", new[]
                {
                    nameof(CommanderConfig.EnableCommandRouter),
                    nameof(CommanderConfig.EnableCommandValidationDebug),
                    nameof(CommanderConfig.AllowBasicChargeWithoutAdvancedDoctrine),
                    nameof(CommanderConfig.AllowNoCommanderBasicHold),
                    nameof(CommanderConfig.AllowNoCommanderBasicFollow),
                    nameof(CommanderConfig.BlockAdvancedCommandsWithoutCommander),
                    nameof(CommanderConfig.EnableNativePrimitiveOrderExecution),
                    nameof(CommanderConfig.CommandValidationDebugLogIntervalSeconds)
                }),
                ("Native cavalry sequence", new[]
                {
                    nameof(CommanderConfig.EnableNativeCavalryChargeSequence),
                    nameof(CommanderConfig.CavalryUseNativeForwardBeforeCharge),
                    nameof(CommanderConfig.CavalryUseNativeChargeCommand),
                    nameof(CommanderConfig.CavalryForwardToChargeDistance),
                    nameof(CommanderConfig.EnableCavalrySequenceDebug)
                }),
                ("Command markers", new[]
                {
                    nameof(CommanderConfig.EnableCommandMarkers),
                    nameof(CommanderConfig.EnableFallbackTextMarkers),
                    nameof(CommanderConfig.DefaultMarkerLifetimeSeconds),
                    nameof(CommanderConfig.ChargeMarkerLifetimeSeconds),
                    nameof(CommanderConfig.ReformMarkerLifetimeSeconds),
                    nameof(CommanderConfig.MarkerRefreshThrottleSeconds)
                }),
                ("Diagnostics", new[]
                {
                    nameof(CommanderConfig.EnableDiagnostics),
                    nameof(CommanderConfig.ShowDiagnosticsInCommanderModeOnly),
                    nameof(CommanderConfig.DiagnosticsToggleKey),
                    nameof(CommanderConfig.DiagnosticsRefreshIntervalSeconds),
                    nameof(CommanderConfig.DiagnosticsTickIntervalSeconds),
                    nameof(CommanderConfig.IncludeDoctrineScores),
                    nameof(CommanderConfig.IncludeEligibility),
                    nameof(CommanderConfig.IncludeRallyAbsorption),
                    nameof(CommanderConfig.IncludeCavalrySequence),
                    nameof(CommanderConfig.IncludeNativeOrderStatus)
                }),
                ("Performance budget (Slice 24)", new[]
                {
                    nameof(CommanderConfig.EnablePerformanceDiagnostics),
                    nameof(CommanderConfig.WarnOnOverBudget),
                    nameof(CommanderConfig.PerformanceWarningThrottleSeconds),
                    nameof(CommanderConfig.TargetingIntervalSeconds),
                    nameof(CommanderConfig.CommanderScanIntervalSeconds),
                    nameof(CommanderConfig.EligibilityScanIntervalSeconds),
                    nameof(CommanderConfig.RallyAbsorptionIntervalSeconds),
                    nameof(CommanderConfig.CavalrySequenceIntervalSeconds),
                    nameof(CommanderConfig.FeedbackTickIntervalSeconds),
                    nameof(CommanderConfig.MarkerTickIntervalSeconds),
                    nameof(CommanderConfig.ConfigReloadCheckIntervalSeconds)
                })
            };

        /// <summary>Builds a plain-text outline for docs or logs.</summary>
        public static string BuildDocumentationOutline()
        {
            var lines = new List<string>
            {
                $"Schema: {SchemaLabel}",
                $"ConfigFileVersion: {CurrentConfigVersion}",
                string.Empty
            };

            foreach ((string title, string[] fields) in DocumentationGroups)
            {
                lines.Add(title + ":");
                foreach (string f in fields)
                {
                    lines.Add($"  - {f}");
                }

                lines.Add(string.Empty);
            }

            return string.Join(Environment.NewLine, lines).TrimEnd();
        }

        private static HashSet<string> BuildKnownRootPropertyNames()
        {
            var set = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (PropertyInfo p in typeof(CommanderConfig).GetProperties(BindingFlags.Instance | BindingFlags.Public))
            {
                set.Add(p.Name);
            }

            return set;
        }
    }
}
