using System;
using System.Collections.Generic;
using System.Text.Json;
using Bannerlord.RTSCameraLite.Input;

namespace Bannerlord.RTSCameraLite.Config
{
    /// <summary>
    /// Validates and sanitizes <see cref="CommanderConfig"/> values after migration (Slice 23).
    /// </summary>
    public static class CommanderConfigValidator
    {
        private const float MinScanIntervalSeconds = 0.1f;
        private const float MinMarkerLifetimeSeconds = 0.1f;
        private const float MinPositiveSpeed = 1e-3f;
        private const float MinSpacing = 1e-3f;

        private static readonly JsonSerializerOptions JsonCloneOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            ReadCommentHandling = JsonCommentHandling.Skip,
            AllowTrailingCommas = true
        };

        public static CommanderConfigValidationResult Validate(CommanderConfig source, CommanderConfig defaults)
        {
            var warnings = new List<string>();
            var errors = new List<string>();
            CommanderConfig defaultsSafe = defaults ?? CommanderConfigDefaults.CreateDefault();

            if (source == null)
            {
                errors.Add("CommanderConfig instance was null; using full defaults.");
                CommanderConfig fallback = Clone(defaultsSafe, JsonCloneOptions);
                return new CommanderConfigValidationResult(
                    isValid: false,
                    usedFallbacks: true,
                    requiresRewrite: true,
                    warnings,
                    errors,
                    fallback);
            }

            CommanderConfig c = Clone(source, JsonCloneOptions);
            bool used = false;

            used |= SanitizeKeybinds(c, defaultsSafe, warnings);
            used |= SanitizeCameraAndMovement(c, defaultsSafe, warnings);
            used |= SanitizeIntervalsAndThrottles(c, defaultsSafe, warnings);
            used |= SanitizeDoctrineWeights(c, warnings);
            used |= SanitizeDisciplineAndRatios(c, defaultsSafe, warnings);
            used |= SanitizeRallyAndAnchor(c, defaultsSafe, warnings);
            used |= SanitizeCavalryDoctrine(c, defaultsSafe, warnings);
            used |= SanitizeMarkersAndDiagnostics(c, defaultsSafe, warnings);
            used |= SanitizePerformanceBudgetIntervals(c, defaultsSafe, warnings);

            ClampDefaultHeightBetweenMinMax(c, warnings, ref used);

            bool isValid = errors.Count == 0;
            return new CommanderConfigValidationResult(
                isValid,
                used,
                requiresRewrite: used,
                warnings,
                errors,
                c);
        }

        private static bool IsFiniteFloat(float f)
        {
            return !float.IsNaN(f) && !float.IsInfinity(f);
        }

        private static CommanderConfig Clone(CommanderConfig c, JsonSerializerOptions o)
        {
            string json = JsonSerializer.Serialize(c, o);
            return JsonSerializer.Deserialize<CommanderConfig>(json, o) ?? CommanderConfigDefaults.CreateDefault();
        }

        private static bool SanitizeKeybinds(CommanderConfig c, CommanderConfig d, List<string> warnings)
        {
            bool used = false;
            string s;
            s = c.ModeActivationKey;
            if (TrySanitizeKey(nameof(CommanderConfig.ModeActivationKey), ref s, d.ModeActivationKey, warnings)) { used = true; }
            c.ModeActivationKey = s;

            s = c.DebugFallbackToggleKey;
            if (TrySanitizeKey(nameof(CommanderConfig.DebugFallbackToggleKey), ref s, d.DebugFallbackToggleKey, warnings)) { used = true; }
            c.DebugFallbackToggleKey = s;

            s = c.MoveForwardKey;
            if (TrySanitizeKey(nameof(CommanderConfig.MoveForwardKey), ref s, d.MoveForwardKey, warnings)) { used = true; }
            c.MoveForwardKey = s;

            s = c.MoveBackKey;
            if (TrySanitizeKey(nameof(CommanderConfig.MoveBackKey), ref s, d.MoveBackKey, warnings)) { used = true; }
            c.MoveBackKey = s;

            s = c.MoveLeftKey;
            if (TrySanitizeKey(nameof(CommanderConfig.MoveLeftKey), ref s, d.MoveLeftKey, warnings)) { used = true; }
            c.MoveLeftKey = s;

            s = c.MoveRightKey;
            if (TrySanitizeKey(nameof(CommanderConfig.MoveRightKey), ref s, d.MoveRightKey, warnings)) { used = true; }
            c.MoveRightKey = s;

            s = c.RotateLeftKey;
            if (TrySanitizeKey(nameof(CommanderConfig.RotateLeftKey), ref s, d.RotateLeftKey, warnings)) { used = true; }
            c.RotateLeftKey = s;

            s = c.RotateRightKey;
            if (TrySanitizeKey(nameof(CommanderConfig.RotateRightKey), ref s, d.RotateRightKey, warnings)) { used = true; }
            c.RotateRightKey = s;

            s = c.FastMoveKey;
            if (TrySanitizeKey(nameof(CommanderConfig.FastMoveKey), ref s, d.FastMoveKey, warnings)) { used = true; }
            c.FastMoveKey = s;

            s = c.ZoomInKey;
            if (TrySanitizeKey(nameof(CommanderConfig.ZoomInKey), ref s, d.ZoomInKey, warnings)) { used = true; }
            c.ZoomInKey = s;

            s = c.ZoomOutKey;
            if (TrySanitizeKey(nameof(CommanderConfig.ZoomOutKey), ref s, d.ZoomOutKey, warnings)) { used = true; }
            c.ZoomOutKey = s;

            s = c.DiagnosticsToggleKey;
            if (TrySanitizeKey(nameof(CommanderConfig.DiagnosticsToggleKey), ref s, d.DiagnosticsToggleKey, warnings)) { used = true; }
            c.DiagnosticsToggleKey = s;

            return used;
        }

        private static bool TrySanitizeKey(string label, ref string current, string fallback, List<string> warnings)
        {
            if (string.IsNullOrWhiteSpace(current) || !CommanderInputKeyParser.TryParse(current, out _))
            {
                warnings.Add($"Keybind '{label}' was invalid or empty; using default '{fallback}'.");
                current = fallback;
                return true;
            }

            return false;
        }

        private static bool SanitizeCameraAndMovement(CommanderConfig c, CommanderConfig d, List<string> warnings)
        {
            bool used = false;
            float v;
            if (TryClampPositive(nameof(CommanderConfig.MoveSpeed), c.MoveSpeed, d.MoveSpeed, warnings, out v)) { c.MoveSpeed = v; used = true; }
            if (TryClampMin(nameof(CommanderConfig.FastMoveMultiplier), c.FastMoveMultiplier, 1f, d.FastMoveMultiplier, warnings, out v)) { c.FastMoveMultiplier = v; used = true; }
            if (TryClampPositive(nameof(CommanderConfig.RotationSpeedDegrees), c.RotationSpeedDegrees, d.RotationSpeedDegrees, warnings, out v)) { c.RotationSpeedDegrees = v; used = true; }
            if (TryClampPositive(nameof(CommanderConfig.ZoomSpeed), c.ZoomSpeed, d.ZoomSpeed, warnings, out v)) { c.ZoomSpeed = v; used = true; }

            if (TryClampMin(nameof(CommanderConfig.MinHeight), c.MinHeight, 2f, d.MinHeight, warnings, out v)) { c.MinHeight = v; used = true; }
            if (!IsFiniteFloat(c.MaxHeight) || c.MaxHeight <= c.MinHeight)
            {
                warnings.Add($"MaxHeight was invalid or not greater than MinHeight ({c.MinHeight}); using default relationship.");
                c.MaxHeight = Math.Max(c.MinHeight + 1f, d.MaxHeight);
                used = true;
            }

            if (!IsFiniteFloat(c.DefaultPitch))
            {
                warnings.Add($"DefaultPitch was non-finite; reset to {d.DefaultPitch}.");
                c.DefaultPitch = d.DefaultPitch;
                used = true;
            }
            else
            {
                c.DefaultPitch = Clamp(c.DefaultPitch, -89f, 89f);
            }

            if (!IsFiniteFloat(c.DefaultHeight))
            {
                warnings.Add($"DefaultHeight was non-finite; reset to {d.DefaultHeight}.");
                c.DefaultHeight = d.DefaultHeight;
                used = true;
            }

            return used;
        }

        private static void ClampDefaultHeightBetweenMinMax(CommanderConfig c, List<string> warnings, ref bool used)
        {
            if (!IsFiniteFloat(c.DefaultHeight))
            {
                return;
            }

            float clamped = Clamp(c.DefaultHeight, c.MinHeight, c.MaxHeight);
            if (Math.Abs(clamped - c.DefaultHeight) > 1e-4f)
            {
                warnings.Add($"DefaultHeight was clamped to [{c.MinHeight}, {c.MaxHeight}] (was {c.DefaultHeight}).");
                c.DefaultHeight = clamped;
                used = true;
            }
        }

        private static bool SanitizeIntervalsAndThrottles(CommanderConfig c, CommanderConfig d, List<string> warnings)
        {
            bool used = false;
            float v;
            if (TryClampMin(nameof(CommanderConfig.DoctrineScanIntervalSeconds), c.DoctrineScanIntervalSeconds, MinScanIntervalSeconds, d.DoctrineScanIntervalSeconds, warnings, out v)) { c.DoctrineScanIntervalSeconds = v; used = true; }
            if (TryClampMin(nameof(CommanderConfig.RallyScanIntervalSeconds), c.RallyScanIntervalSeconds, MinScanIntervalSeconds, d.RallyScanIntervalSeconds, warnings, out v)) { c.RallyScanIntervalSeconds = v; used = true; }
            if (TryClampMin(nameof(CommanderConfig.CommandValidationDebugLogIntervalSeconds), c.CommandValidationDebugLogIntervalSeconds, 0f, d.CommandValidationDebugLogIntervalSeconds, warnings, out v)) { c.CommandValidationDebugLogIntervalSeconds = v; used = true; }
            if (TryClampMin(nameof(CommanderConfig.MarkerRefreshThrottleSeconds), c.MarkerRefreshThrottleSeconds, 0f, d.MarkerRefreshThrottleSeconds, warnings, out v)) { c.MarkerRefreshThrottleSeconds = v; used = true; }
            if (TryClampMin(nameof(CommanderConfig.DiagnosticsRefreshIntervalSeconds), c.DiagnosticsRefreshIntervalSeconds, 0f, d.DiagnosticsRefreshIntervalSeconds, warnings, out v)) { c.DiagnosticsRefreshIntervalSeconds = v; used = true; }
            if (TryClampMin(nameof(CommanderConfig.SlotReassignmentCooldownSeconds), c.SlotReassignmentCooldownSeconds, 0f, d.SlotReassignmentCooldownSeconds, warnings, out v)) { c.SlotReassignmentCooldownSeconds = v; used = true; }
            if (TryClampMin(nameof(CommanderConfig.CavalryReformCooldownSeconds), c.CavalryReformCooldownSeconds, 0f, d.CavalryReformCooldownSeconds, warnings, out v)) { c.CavalryReformCooldownSeconds = v; used = true; }
            return used;
        }

        private static bool SanitizeDoctrineWeights(CommanderConfig c, List<string> warnings)
        {
            bool used = false;
            float v;
            if (TryClampMinZero(nameof(CommanderConfig.MoraleWeight), c.MoraleWeight, warnings, out v)) { c.MoraleWeight = v; used = true; }
            if (TryClampMinZero(nameof(CommanderConfig.TrainingWeight), c.TrainingWeight, warnings, out v)) { c.TrainingWeight = v; used = true; }
            if (TryClampMinZero(nameof(CommanderConfig.EquipmentWeight), c.EquipmentWeight, warnings, out v)) { c.EquipmentWeight = v; used = true; }
            if (TryClampMinZero(nameof(CommanderConfig.CommanderWeight), c.CommanderWeight, warnings, out v)) { c.CommanderWeight = v; used = true; }
            if (TryClampMinZero(nameof(CommanderConfig.CohesionWeight), c.CohesionWeight, warnings, out v)) { c.CohesionWeight = v; used = true; }
            if (TryClampMinZero(nameof(CommanderConfig.RankWeight), c.RankWeight, warnings, out v)) { c.RankWeight = v; used = true; }
            if (TryClampMinZero(nameof(CommanderConfig.CasualtyShockPenaltyWeight), c.CasualtyShockPenaltyWeight, warnings, out v)) { c.CasualtyShockPenaltyWeight = v; used = true; }
            return used;
        }

        private static bool SanitizeDisciplineAndRatios(CommanderConfig c, CommanderConfig d, List<string> warnings)
        {
            bool used = false;
            float v;
            if (TryClamp01(nameof(CommanderConfig.MinimumCommandAuthorityScore), c.MinimumCommandAuthorityScore, d.MinimumCommandAuthorityScore, warnings, out v)) { c.MinimumCommandAuthorityScore = v; used = true; }
            if (TryClamp01(nameof(CommanderConfig.BasicLineMinimumDiscipline), c.BasicLineMinimumDiscipline, d.BasicLineMinimumDiscipline, warnings, out v)) { c.BasicLineMinimumDiscipline = v; used = true; }
            if (TryClamp01(nameof(CommanderConfig.LooseMinimumDiscipline), c.LooseMinimumDiscipline, d.LooseMinimumDiscipline, warnings, out v)) { c.LooseMinimumDiscipline = v; used = true; }
            if (TryClamp01(nameof(CommanderConfig.ShieldWallMinimumDiscipline), c.ShieldWallMinimumDiscipline, d.ShieldWallMinimumDiscipline, warnings, out v)) { c.ShieldWallMinimumDiscipline = v; used = true; }
            if (TryClamp01(nameof(CommanderConfig.SquareMinimumDiscipline), c.SquareMinimumDiscipline, d.SquareMinimumDiscipline, warnings, out v)) { c.SquareMinimumDiscipline = v; used = true; }
            if (TryClamp01(nameof(CommanderConfig.CircleMinimumDiscipline), c.CircleMinimumDiscipline, d.CircleMinimumDiscipline, warnings, out v)) { c.CircleMinimumDiscipline = v; used = true; }
            if (TryClamp01(nameof(CommanderConfig.AdvancedAdaptiveMinimumDiscipline), c.AdvancedAdaptiveMinimumDiscipline, d.AdvancedAdaptiveMinimumDiscipline, warnings, out v)) { c.AdvancedAdaptiveMinimumDiscipline = v; used = true; }
            if (TryClamp01(nameof(CommanderConfig.MinimumShieldRatioForShieldWall), c.MinimumShieldRatioForShieldWall, d.MinimumShieldRatioForShieldWall, warnings, out v)) { c.MinimumShieldRatioForShieldWall = v; used = true; }
            if (TryClamp01(nameof(CommanderConfig.MinimumPolearmOrShieldRatioForSquare), c.MinimumPolearmOrShieldRatioForSquare, d.MinimumPolearmOrShieldRatioForSquare, warnings, out v)) { c.MinimumPolearmOrShieldRatioForSquare = v; used = true; }
            if (TryClamp01(nameof(CommanderConfig.MinimumMountedRatioForMountedWide), c.MinimumMountedRatioForMountedWide, d.MinimumMountedRatioForMountedWide, warnings, out v)) { c.MinimumMountedRatioForMountedWide = v; used = true; }
            if (TryClamp01(nameof(CommanderConfig.MinimumHorseArcherRatioForHorseArcherLoose), c.MinimumHorseArcherRatioForHorseArcherLoose, d.MinimumHorseArcherRatioForHorseArcherLoose, warnings, out v)) { c.MinimumHorseArcherRatioForHorseArcherLoose = v; used = true; }
            return used;
        }

        private static bool SanitizeRallyAndAnchor(CommanderConfig c, CommanderConfig d, List<string> warnings)
        {
            bool used = false;
            float v;
            if (TryClampPositive(nameof(CommanderConfig.CommanderRallyRadius), c.CommanderRallyRadius, d.CommanderRallyRadius, warnings, out v)) { c.CommanderRallyRadius = v; used = true; }
            if (TryClampPositive(nameof(CommanderConfig.CommanderAbsorptionRadius), c.CommanderAbsorptionRadius, d.CommanderAbsorptionRadius, warnings, out v)) { c.CommanderAbsorptionRadius = v; used = true; }
            if (TryClampPositive(nameof(CommanderConfig.FormationSlotRadius), c.FormationSlotRadius, d.FormationSlotRadius, warnings, out v)) { c.FormationSlotRadius = v; used = true; }
            if (TryClampPositive(nameof(CommanderConfig.CohesionBreakRadius), c.CohesionBreakRadius, d.CohesionBreakRadius, warnings, out v)) { c.CohesionBreakRadius = v; used = true; }
            if (TryClampMin(nameof(CommanderConfig.AnchorAllowedRadius), c.AnchorAllowedRadius, 0f, d.AnchorAllowedRadius, warnings, out v)) { c.AnchorAllowedRadius = v; used = true; }
            if (TryClampMin(nameof(CommanderConfig.DefaultCommanderBackOffset), c.DefaultCommanderBackOffset, 0f, d.DefaultCommanderBackOffset, warnings, out v)) { c.DefaultCommanderBackOffset = v; used = true; }
            if (TryClampMin(nameof(CommanderConfig.ShieldWallCommanderBackOffset), c.ShieldWallCommanderBackOffset, 0f, d.ShieldWallCommanderBackOffset, warnings, out v)) { c.ShieldWallCommanderBackOffset = v; used = true; }
            if (TryClampMin(nameof(CommanderConfig.ArcherCommanderBackOffset), c.ArcherCommanderBackOffset, 0f, d.ArcherCommanderBackOffset, warnings, out v)) { c.ArcherCommanderBackOffset = v; used = true; }
            if (TryClampMin(nameof(CommanderConfig.CavalryCommanderBackOffset), c.CavalryCommanderBackOffset, 0f, d.CavalryCommanderBackOffset, warnings, out v)) { c.CavalryCommanderBackOffset = v; used = true; }
            if (TryClampMin(nameof(CommanderConfig.SkirmisherCommanderBackOffset), c.SkirmisherCommanderBackOffset, 0f, d.SkirmisherCommanderBackOffset, warnings, out v)) { c.SkirmisherCommanderBackOffset = v; used = true; }
            return used;
        }

        private static bool SanitizeCavalryDoctrine(CommanderConfig c, CommanderConfig d, List<string> warnings)
        {
            bool used = false;
            float v;
            if (TryClampMin(nameof(CommanderConfig.CavalryLateralSpacing), c.CavalryLateralSpacing, MinSpacing, d.CavalryLateralSpacing, warnings, out v)) { c.CavalryLateralSpacing = v; used = true; }
            if (TryClampMin(nameof(CommanderConfig.CavalryDepthSpacing), c.CavalryDepthSpacing, MinSpacing, d.CavalryDepthSpacing, warnings, out v)) { c.CavalryDepthSpacing = v; used = true; }
            if (TryClampMin(nameof(CommanderConfig.HorseArcherLateralSpacing), c.HorseArcherLateralSpacing, MinSpacing, d.HorseArcherLateralSpacing, warnings, out v)) { c.HorseArcherLateralSpacing = v; used = true; }
            if (TryClampMin(nameof(CommanderConfig.HorseArcherDepthSpacing), c.HorseArcherDepthSpacing, MinSpacing, d.HorseArcherDepthSpacing, warnings, out v)) { c.HorseArcherDepthSpacing = v; used = true; }
            if (TryClampMin(nameof(CommanderConfig.CavalryReleaseLockDistance), c.CavalryReleaseLockDistance, MinSpacing, d.CavalryReleaseLockDistance, warnings, out v)) { c.CavalryReleaseLockDistance = v; used = true; }
            float releaseDistance = c.CavalryReleaseLockDistance;
            if (!IsFiniteFloat(c.CavalryReformDistanceFromAttackedFormation) || c.CavalryReformDistanceFromAttackedFormation < MinSpacing)
            {
                float fallback = Math.Max(releaseDistance, d.CavalryReformDistanceFromAttackedFormation);
                warnings.Add(
                    $"CavalryReformDistanceFromAttackedFormation was invalid or too small; using {fallback} (at least release distance {releaseDistance}).");
                c.CavalryReformDistanceFromAttackedFormation = fallback;
                used = true;
            }
            else if (c.CavalryReformDistanceFromAttackedFormation < releaseDistance)
            {
                warnings.Add(
                    $"CavalryReformDistanceFromAttackedFormation ({c.CavalryReformDistanceFromAttackedFormation}) was below CavalryReleaseLockDistance ({releaseDistance}); clamped to release distance.");
                c.CavalryReformDistanceFromAttackedFormation = releaseDistance;
                used = true;
            }

            if (TryClampMin(nameof(CommanderConfig.CavalryMinimumEnemyDistanceToReform), c.CavalryMinimumEnemyDistanceToReform, 0f, d.CavalryMinimumEnemyDistanceToReform, warnings, out v)) { c.CavalryMinimumEnemyDistanceToReform = v; used = true; }
            if (TryClampMin(nameof(CommanderConfig.CavalryImpactEnemyDistance), c.CavalryImpactEnemyDistance, 0f, d.CavalryImpactEnemyDistance, warnings, out v)) { c.CavalryImpactEnemyDistance = v; used = true; }
            if (TryClamp01(nameof(CommanderConfig.CavalryImpactSpeedDropThreshold), c.CavalryImpactSpeedDropThreshold, d.CavalryImpactSpeedDropThreshold, warnings, out v)) { c.CavalryImpactSpeedDropThreshold = v; used = true; }
            if (TryClamp01(nameof(CommanderConfig.CavalryImpactAgentRatio), c.CavalryImpactAgentRatio, d.CavalryImpactAgentRatio, warnings, out v)) { c.CavalryImpactAgentRatio = v; used = true; }
            if (TryClampPositive(nameof(CommanderConfig.CavalryForwardToChargeDistance), c.CavalryForwardToChargeDistance, d.CavalryForwardToChargeDistance, warnings, out v)) { c.CavalryForwardToChargeDistance = v; used = true; }
            return used;
        }

        private static bool SanitizeMarkersAndDiagnostics(CommanderConfig c, CommanderConfig d, List<string> warnings)
        {
            bool used = false;
            float v;
            if (TryClampMin(nameof(CommanderConfig.DefaultMarkerLifetimeSeconds), c.DefaultMarkerLifetimeSeconds, MinMarkerLifetimeSeconds, d.DefaultMarkerLifetimeSeconds, warnings, out v)) { c.DefaultMarkerLifetimeSeconds = v; used = true; }
            if (TryClampMin(nameof(CommanderConfig.ChargeMarkerLifetimeSeconds), c.ChargeMarkerLifetimeSeconds, MinMarkerLifetimeSeconds, d.ChargeMarkerLifetimeSeconds, warnings, out v)) { c.ChargeMarkerLifetimeSeconds = v; used = true; }
            if (TryClampMin(nameof(CommanderConfig.ReformMarkerLifetimeSeconds), c.ReformMarkerLifetimeSeconds, MinMarkerLifetimeSeconds, d.ReformMarkerLifetimeSeconds, warnings, out v)) { c.ReformMarkerLifetimeSeconds = v; used = true; }
            return used;
        }

        private static bool SanitizePerformanceBudgetIntervals(CommanderConfig c, CommanderConfig d, List<string> warnings)
        {
            bool used = false;
            float v;
            if (TryClampMin(nameof(CommanderConfig.PerformanceWarningThrottleSeconds), c.PerformanceWarningThrottleSeconds, 0.5f, d.PerformanceWarningThrottleSeconds, warnings, out v)) { c.PerformanceWarningThrottleSeconds = v; used = true; }
            if (TryClampMin(nameof(CommanderConfig.TargetingIntervalSeconds), c.TargetingIntervalSeconds, 0.01f, d.TargetingIntervalSeconds, warnings, out v)) { c.TargetingIntervalSeconds = v; used = true; }
            if (TryClampMin(nameof(CommanderConfig.CommanderScanIntervalSeconds), c.CommanderScanIntervalSeconds, MinScanIntervalSeconds, d.CommanderScanIntervalSeconds, warnings, out v)) { c.CommanderScanIntervalSeconds = v; used = true; }
            if (TryClampMin(nameof(CommanderConfig.EligibilityScanIntervalSeconds), c.EligibilityScanIntervalSeconds, MinScanIntervalSeconds, d.EligibilityScanIntervalSeconds, warnings, out v)) { c.EligibilityScanIntervalSeconds = v; used = true; }
            if (TryClampMin(nameof(CommanderConfig.RallyAbsorptionIntervalSeconds), c.RallyAbsorptionIntervalSeconds, MinScanIntervalSeconds, d.RallyAbsorptionIntervalSeconds, warnings, out v)) { c.RallyAbsorptionIntervalSeconds = v; used = true; }
            if (TryClampMin(nameof(CommanderConfig.CavalrySequenceIntervalSeconds), c.CavalrySequenceIntervalSeconds, 0.01f, d.CavalrySequenceIntervalSeconds, warnings, out v)) { c.CavalrySequenceIntervalSeconds = v; used = true; }
            if (TryClampMin(nameof(CommanderConfig.FeedbackTickIntervalSeconds), c.FeedbackTickIntervalSeconds, 0f, d.FeedbackTickIntervalSeconds, warnings, out v)) { c.FeedbackTickIntervalSeconds = v; used = true; }
            if (TryClampMin(nameof(CommanderConfig.MarkerTickIntervalSeconds), c.MarkerTickIntervalSeconds, 0.01f, d.MarkerTickIntervalSeconds, warnings, out v)) { c.MarkerTickIntervalSeconds = v; used = true; }
            if (!IsFiniteFloat(c.DiagnosticsTickIntervalSeconds) || c.DiagnosticsTickIntervalSeconds < 0f)
            {
                c.DiagnosticsTickIntervalSeconds = d.DiagnosticsTickIntervalSeconds;
                used = true;
            }
            else if (c.DiagnosticsTickIntervalSeconds > 0f && c.DiagnosticsTickIntervalSeconds < MinScanIntervalSeconds)
            {
                warnings.Add($"{nameof(CommanderConfig.DiagnosticsTickIntervalSeconds)} below {MinScanIntervalSeconds}s when non-zero; clamped.");
                c.DiagnosticsTickIntervalSeconds = MinScanIntervalSeconds;
                used = true;
            }

            if (TryClampMin(nameof(CommanderConfig.ConfigReloadCheckIntervalSeconds), c.ConfigReloadCheckIntervalSeconds, MinScanIntervalSeconds, d.ConfigReloadCheckIntervalSeconds, warnings, out v)) { c.ConfigReloadCheckIntervalSeconds = v; used = true; }
            return used;
        }

        private static bool TryClampPositive(string label, float current, float fallback, List<string> warnings, out float resolved)
        {
            if (!IsFiniteFloat(current) || current <= MinPositiveSpeed)
            {
                warnings.Add($"'{label}' must be > 0; using default {fallback}.");
                resolved = fallback;
                return true;
            }

            resolved = current;
            return false;
        }

        private static bool TryClampMin(string label, float current, float min, float fallback, List<string> warnings, out float resolved)
        {
            if (!IsFiniteFloat(current) || current < min)
            {
                warnings.Add($"'{label}' was invalid or below minimum {min}; using default {fallback}.");
                resolved = fallback;
                return true;
            }

            resolved = current;
            return false;
        }

        private static bool TryClampMinZero(string label, float current, List<string> warnings, out float resolved)
        {
            if (!IsFiniteFloat(current) || current < 0f)
            {
                warnings.Add($"'{label}' doctrine weight was invalid or negative; clamped to 0.");
                resolved = 0f;
                return true;
            }

            resolved = current;
            return false;
        }

        private static bool TryClamp01(string label, float current, float fallback, List<string> warnings, out float resolved)
        {
            if (!IsFiniteFloat(current))
            {
                warnings.Add($"'{label}' was non-finite; using default {fallback}.");
                resolved = fallback;
                return true;
            }

            if (current < 0f || current > 1f)
            {
                float clamped = Clamp(current, 0f, 1f);
                warnings.Add($"'{label}' was outside [0,1]; clamped from {current} to {clamped}.");
                resolved = clamped;
                return true;
            }

            resolved = current;
            return false;
        }

        private static float Clamp(float v, float lo, float hi)
        {
            if (v < lo)
            {
                return lo;
            }

            if (v > hi)
            {
                return hi;
            }

            return v;
        }
    }
}
