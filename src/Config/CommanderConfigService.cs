using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Bannerlord.RTSCameraLite.Core;
using TaleWorlds.Library;

namespace Bannerlord.RTSCameraLite.Config
{
    /// <summary>
    /// Loads commander JSON from the module folder; creates a default file when missing; never throws to callers.
    /// </summary>
    public static class CommanderConfigService
    {
        private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            ReadCommentHandling = JsonCommentHandling.Skip,
            AllowTrailingCommas = true
        };

        private static readonly JsonSerializerOptions WriteJsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true
        };

        public static ConfigLoadResult LoadOrCreate()
        {
            string configPath;
            try
            {
                configPath = ResolveConfigPath();
            }
            catch (Exception ex)
            {
                ModLogger.LogWarningOnce(
                    "commander_config_path",
                    $"{ModConstants.ModuleId}: could not resolve config path ({ex.Message}); using defaults.");
                return new ConfigLoadResult(
                    loaded: false,
                    usedDefaults: true,
                    createdDefaultFile: false,
                    message: ex.Message,
                    CommanderConfigDefaults.CreateDefault());
            }

            string directory = Path.GetDirectoryName(configPath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                try
                {
                    Directory.CreateDirectory(directory);
                }
                catch (Exception ex)
                {
                    ModLogger.LogWarningOnce(
                        "commander_config_mkdir",
                        $"{ModConstants.ModuleId}: could not create config directory ({ex.Message}); using defaults.");
                    return new ConfigLoadResult(
                        loaded: false,
                        usedDefaults: true,
                        createdDefaultFile: false,
                        message: ex.Message,
                        CommanderConfigDefaults.CreateDefault());
                }
            }

            if (!File.Exists(configPath))
            {
                CommanderConfig defaults = CommanderConfigDefaults.CreateDefault();
                try
                {
                    CommanderConfigValidationResult createdValidation = CommanderConfigValidator.Validate(defaults, defaults);
                    WriteConfig(configPath, createdValidation.SanitizedConfig);
                    ModLogger.LogDebug($"{ModConstants.ModuleId}: created default {configPath}");
                    return new ConfigLoadResult(
                        loaded: true,
                        usedDefaults: false,
                        createdDefaultFile: true,
                        message: "Created default commander_config.json",
                        createdValidation.SanitizedConfig,
                        createdValidation);
                }
                catch (Exception ex)
                {
                    ModLogger.LogWarningOnce(
                        "commander_config_create",
                        $"{ModConstants.ModuleId}: could not write default config ({ex.Message}); using in-memory defaults.");
                    return new ConfigLoadResult(
                        loaded: false,
                        usedDefaults: true,
                        createdDefaultFile: false,
                        message: ex.Message,
                        defaults);
                }
            }

            try
            {
                string json = File.ReadAllText(configPath);
                CommanderConfig parsed = JsonSerializer.Deserialize<CommanderConfig>(json, JsonOptions);
                if (parsed == null)
                {
                    throw new InvalidOperationException("deserialized config is null");
                }

                ApplyOmittedSlice7PolicyDefaults(json, parsed);
                ApplyOmittedSlice25RuntimeHookDefaults(json, parsed);
                CommanderConfigDefaults.HarmonizeLegacyDetectionFields(parsed);
                ApplyOmittedSlice9AnchorDefaults(json, parsed);
                ApplyOmittedSlice10DoctrineDefaults(json, parsed);
                CommanderConfigDefaults.HarmonizeLegacyDoctrineFields(parsed);
                ApplyOmittedSlice11EligibilityDefaults(json, parsed);
                CommanderConfigDefaults.HarmonizeLegacyEligibilityFields(parsed);
                ApplyOmittedSlice12RallyDefaults(json, parsed);
                CommanderConfigDefaults.HarmonizeLegacyRallyFields(parsed);
                ApplyOmittedSlice13CavalryDefaults(json, parsed);
                CommanderConfigDefaults.HarmonizeLegacyCavalryDoctrineFields(parsed);
                ApplyOmittedSlice14NativeOrderDefaults(json, parsed);
                ApplyOmittedSlice15CommandRouterDefaults(json, parsed);
                CommanderConfigDefaults.HarmonizeLegacyCommandRouterFields(parsed);
                ApplyOmittedSlice16NativeCavalrySequenceDefaults(json, parsed);
                ApplyOmittedSlice19CommandMarkerDefaults(json, parsed);
                ApplyOmittedSlice20DiagnosticsDefaults(json, parsed);
                ApplyOmittedSlice24PerformanceBudgetDefaults(json, parsed);

                CommanderConfig defaults = CommanderConfigDefaults.CreateDefault();
                CommanderConfigValidationResult validation = FinalizeLoadedConfig(configPath, json, parsed, defaults);
                return new ConfigLoadResult(
                    loaded: true,
                    usedDefaults: false,
                    createdDefaultFile: false,
                    message: "OK",
                    validation.SanitizedConfig,
                    validation);
            }
            catch (Exception ex)
            {
                ModLogger.LogWarningOnce(
                    "commander_config_fallback",
                    $"{ModConstants.ModuleId}: commander_config.json invalid ({ex.Message}); using defaults.");
                return new ConfigLoadResult(
                    loaded: false,
                    usedDefaults: true,
                    createdDefaultFile: false,
                    message: ex.Message,
                    CommanderConfigDefaults.CreateDefault());
            }
        }

        /// <summary>
        /// System.Text.Json sets omitted booleans to false. Restore Slice 7 policy defaults when keys are absent from the file.
        /// </summary>
        private static void ApplyOmittedSlice7PolicyDefaults(string json, CommanderConfig parsed)
        {
            if (parsed == null || string.IsNullOrWhiteSpace(json))
            {
                return;
            }

            try
            {
                using JsonDocument doc = JsonDocument.Parse(json);
                if (doc.RootElement.ValueKind != JsonValueKind.Object)
                {
                    return;
                }

                JsonElement root = doc.RootElement;
                CommanderConfig d = CommanderConfigDefaults.CreateDefault();
                if (!JsonHasPropertyIgnoreCase(root, nameof(CommanderConfig.AllowNativeOrdersWhenCommanderModeDisabled)))
                {
                    parsed.AllowNativeOrdersWhenCommanderModeDisabled = d.AllowNativeOrdersWhenCommanderModeDisabled;
                }

                if (!JsonHasPropertyIgnoreCase(root, nameof(CommanderConfig.EnableInputOwnershipGuard)))
                {
                    parsed.EnableInputOwnershipGuard = d.EnableInputOwnershipGuard;
                }

                if (!JsonHasPropertyIgnoreCase(root, nameof(CommanderConfig.SuppressNativeMovementInCommanderMode)))
                {
                    parsed.SuppressNativeMovementInCommanderMode = d.SuppressNativeMovementInCommanderMode;
                }

                if (!JsonHasPropertyIgnoreCase(root, nameof(CommanderConfig.SuppressNativeCombatInCommanderMode)))
                {
                    parsed.SuppressNativeCombatInCommanderMode = d.SuppressNativeCombatInCommanderMode;
                }
            }
            catch
            {
                // Primary deserialize already succeeded; ignore merge edge cases.
            }
        }

        private static bool JsonHasPropertyIgnoreCase(JsonElement root, string name)
        {
            foreach (JsonProperty p in root.EnumerateObject())
            {
                if (string.Equals(p.Name, name, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }

        private static void ApplyOmittedSlice25RuntimeHookDefaults(string json, CommanderConfig parsed)
        {
            if (parsed == null || string.IsNullOrWhiteSpace(json))
            {
                return;
            }

            try
            {
                using JsonDocument doc = JsonDocument.Parse(json);
                if (doc.RootElement.ValueKind != JsonValueKind.Object)
                {
                    return;
                }

                JsonElement root = doc.RootElement;
                if (!JsonHasPropertyIgnoreCase(root, nameof(CommanderConfig.EnableMissionRuntimeHooks)))
                {
                    parsed.EnableMissionRuntimeHooks = false;
                }
            }
            catch
            {
                // Ignore merge failures.
            }
        }

        /// <summary>
        /// Omitted Slice 9 keys deserialize to 0 / false — restore anchor defaults when absent from JSON.
        /// </summary>
        private static void ApplyOmittedSlice9AnchorDefaults(string json, CommanderConfig parsed)
        {
            if (parsed == null || string.IsNullOrWhiteSpace(json))
            {
                return;
            }

            try
            {
                using JsonDocument doc = JsonDocument.Parse(json);
                if (doc.RootElement.ValueKind != JsonValueKind.Object)
                {
                    return;
                }

                JsonElement root = doc.RootElement;
                CommanderConfig d = CommanderConfigDefaults.CreateDefault();
                if (!JsonHasPropertyIgnoreCase(root, nameof(CommanderConfig.DefaultCommanderBackOffset)))
                {
                    parsed.DefaultCommanderBackOffset = d.DefaultCommanderBackOffset;
                }

                if (!JsonHasPropertyIgnoreCase(root, nameof(CommanderConfig.ShieldWallCommanderBackOffset)))
                {
                    parsed.ShieldWallCommanderBackOffset = d.ShieldWallCommanderBackOffset;
                }

                if (!JsonHasPropertyIgnoreCase(root, nameof(CommanderConfig.ArcherCommanderBackOffset)))
                {
                    parsed.ArcherCommanderBackOffset = d.ArcherCommanderBackOffset;
                }

                if (!JsonHasPropertyIgnoreCase(root, nameof(CommanderConfig.CavalryCommanderBackOffset)))
                {
                    parsed.CavalryCommanderBackOffset = d.CavalryCommanderBackOffset;
                }

                if (!JsonHasPropertyIgnoreCase(root, nameof(CommanderConfig.SkirmisherCommanderBackOffset)))
                {
                    parsed.SkirmisherCommanderBackOffset = d.SkirmisherCommanderBackOffset;
                }

                if (!JsonHasPropertyIgnoreCase(root, nameof(CommanderConfig.AnchorAllowedRadius)))
                {
                    parsed.AnchorAllowedRadius = d.AnchorAllowedRadius;
                }

                if (!JsonHasPropertyIgnoreCase(root, nameof(CommanderConfig.EnableCommanderAnchorDebug)))
                {
                    parsed.EnableCommanderAnchorDebug = d.EnableCommanderAnchorDebug;
                }
            }
            catch
            {
                // Ignore merge failures.
            }
        }

        /// <summary>
        /// Omitted Slice 10 keys deserialize to 0 / false — restore doctrine defaults when absent from JSON.
        /// </summary>
        private static void ApplyOmittedSlice10DoctrineDefaults(string json, CommanderConfig parsed)
        {
            if (parsed == null || string.IsNullOrWhiteSpace(json))
            {
                return;
            }

            try
            {
                using JsonDocument doc = JsonDocument.Parse(json);
                if (doc.RootElement.ValueKind != JsonValueKind.Object)
                {
                    return;
                }

                JsonElement root = doc.RootElement;
                CommanderConfig d = CommanderConfigDefaults.CreateDefault();
                if (!JsonHasPropertyIgnoreCase(root, nameof(CommanderConfig.MoraleWeight)))
                {
                    parsed.MoraleWeight = d.MoraleWeight;
                }

                if (!JsonHasPropertyIgnoreCase(root, nameof(CommanderConfig.TrainingWeight)))
                {
                    parsed.TrainingWeight = d.TrainingWeight;
                }

                if (!JsonHasPropertyIgnoreCase(root, nameof(CommanderConfig.EquipmentWeight)))
                {
                    parsed.EquipmentWeight = d.EquipmentWeight;
                }

                if (!JsonHasPropertyIgnoreCase(root, nameof(CommanderConfig.CommanderWeight)))
                {
                    parsed.CommanderWeight = d.CommanderWeight;
                }

                if (!JsonHasPropertyIgnoreCase(root, nameof(CommanderConfig.CohesionWeight)))
                {
                    parsed.CohesionWeight = d.CohesionWeight;
                }

                if (!JsonHasPropertyIgnoreCase(root, nameof(CommanderConfig.RankWeight)))
                {
                    parsed.RankWeight = d.RankWeight;
                }

                if (!JsonHasPropertyIgnoreCase(root, nameof(CommanderConfig.CasualtyShockPenaltyWeight)))
                {
                    parsed.CasualtyShockPenaltyWeight = d.CasualtyShockPenaltyWeight;
                }

                if (!JsonHasPropertyIgnoreCase(root, nameof(CommanderConfig.EnableDoctrineDebug)))
                {
                    parsed.EnableDoctrineDebug = d.EnableDoctrineDebug;
                }

                if (!JsonHasPropertyIgnoreCase(root, nameof(CommanderConfig.DoctrineScanIntervalSeconds)))
                {
                    parsed.DoctrineScanIntervalSeconds = d.DoctrineScanIntervalSeconds;
                }
            }
            catch
            {
                // Ignore merge failures.
            }
        }

        /// <summary>
        /// Omitted Slice 12 keys deserialize to 0 / false — restore rally defaults when absent from JSON.
        /// </summary>
        private static void ApplyOmittedSlice12RallyDefaults(string json, CommanderConfig parsed)
        {
            if (parsed == null || string.IsNullOrWhiteSpace(json))
            {
                return;
            }

            try
            {
                using JsonDocument doc = JsonDocument.Parse(json);
                if (doc.RootElement.ValueKind != JsonValueKind.Object)
                {
                    return;
                }

                JsonElement root = doc.RootElement;
                CommanderConfig d = CommanderConfigDefaults.CreateDefault();
                if (!JsonHasPropertyIgnoreCase(root, nameof(CommanderConfig.CommanderRallyRadius)))
                {
                    parsed.CommanderRallyRadius = d.CommanderRallyRadius;
                }

                if (!JsonHasPropertyIgnoreCase(root, nameof(CommanderConfig.CommanderAbsorptionRadius)))
                {
                    parsed.CommanderAbsorptionRadius = d.CommanderAbsorptionRadius;
                }

                if (!JsonHasPropertyIgnoreCase(root, nameof(CommanderConfig.FormationSlotRadius)))
                {
                    parsed.FormationSlotRadius = d.FormationSlotRadius;
                }

                if (!JsonHasPropertyIgnoreCase(root, nameof(CommanderConfig.CohesionBreakRadius)))
                {
                    parsed.CohesionBreakRadius = d.CohesionBreakRadius;
                }

                if (!JsonHasPropertyIgnoreCase(root, nameof(CommanderConfig.SlotReassignmentCooldownSeconds)))
                {
                    parsed.SlotReassignmentCooldownSeconds = d.SlotReassignmentCooldownSeconds;
                }

                if (!JsonHasPropertyIgnoreCase(root, nameof(CommanderConfig.RallyScanIntervalSeconds)))
                {
                    parsed.RallyScanIntervalSeconds = d.RallyScanIntervalSeconds;
                }

                if (!JsonHasPropertyIgnoreCase(root, nameof(CommanderConfig.EnableRallyAbsorptionDebug)))
                {
                    parsed.EnableRallyAbsorptionDebug = d.EnableRallyAbsorptionDebug;
                }
            }
            catch
            {
                // Ignore merge failures.
            }
        }

        /// <summary>
        /// Omitted Slice 13 keys deserialize to 0 / false — restore cavalry doctrine defaults when absent from JSON.
        /// </summary>
        private static void ApplyOmittedSlice13CavalryDefaults(string json, CommanderConfig parsed)
        {
            if (parsed == null || string.IsNullOrWhiteSpace(json))
            {
                return;
            }

            try
            {
                using JsonDocument doc = JsonDocument.Parse(json);
                if (doc.RootElement.ValueKind != JsonValueKind.Object)
                {
                    return;
                }

                JsonElement root = doc.RootElement;
                CommanderConfig d = CommanderConfigDefaults.CreateDefault();
                if (!JsonHasPropertyIgnoreCase(root, nameof(CommanderConfig.CavalryLateralSpacing)))
                {
                    parsed.CavalryLateralSpacing = d.CavalryLateralSpacing;
                }

                if (!JsonHasPropertyIgnoreCase(root, nameof(CommanderConfig.CavalryDepthSpacing)))
                {
                    parsed.CavalryDepthSpacing = d.CavalryDepthSpacing;
                }

                if (!JsonHasPropertyIgnoreCase(root, nameof(CommanderConfig.HorseArcherLateralSpacing)))
                {
                    parsed.HorseArcherLateralSpacing = d.HorseArcherLateralSpacing;
                }

                if (!JsonHasPropertyIgnoreCase(root, nameof(CommanderConfig.HorseArcherDepthSpacing)))
                {
                    parsed.HorseArcherDepthSpacing = d.HorseArcherDepthSpacing;
                }

                if (!JsonHasPropertyIgnoreCase(root, nameof(CommanderConfig.CavalryReleaseLockDistance)))
                {
                    parsed.CavalryReleaseLockDistance = d.CavalryReleaseLockDistance;
                }

                if (!JsonHasPropertyIgnoreCase(root, nameof(CommanderConfig.CavalryReformDistanceFromAttackedFormation)))
                {
                    parsed.CavalryReformDistanceFromAttackedFormation = d.CavalryReformDistanceFromAttackedFormation;
                }

                if (!JsonHasPropertyIgnoreCase(root, nameof(CommanderConfig.CavalryReformCooldownSeconds)))
                {
                    parsed.CavalryReformCooldownSeconds = d.CavalryReformCooldownSeconds;
                }

                if (!JsonHasPropertyIgnoreCase(root, nameof(CommanderConfig.CavalryMinimumEnemyDistanceToReform)))
                {
                    parsed.CavalryMinimumEnemyDistanceToReform = d.CavalryMinimumEnemyDistanceToReform;
                }

                if (!JsonHasPropertyIgnoreCase(root, nameof(CommanderConfig.CavalryImpactEnemyDistance)))
                {
                    parsed.CavalryImpactEnemyDistance = d.CavalryImpactEnemyDistance;
                }

                if (!JsonHasPropertyIgnoreCase(root, nameof(CommanderConfig.CavalryImpactSpeedDropThreshold)))
                {
                    parsed.CavalryImpactSpeedDropThreshold = d.CavalryImpactSpeedDropThreshold;
                }

                if (!JsonHasPropertyIgnoreCase(root, nameof(CommanderConfig.CavalryImpactAgentRatio)))
                {
                    parsed.CavalryImpactAgentRatio = d.CavalryImpactAgentRatio;
                }

                if (!JsonHasPropertyIgnoreCase(root, nameof(CommanderConfig.EnableCavalryDoctrineDebug)))
                {
                    parsed.EnableCavalryDoctrineDebug = d.EnableCavalryDoctrineDebug;
                }

                if (!JsonHasPropertyIgnoreCase(root, nameof(CommanderConfig.AllowCavalryReformWithoutCommander)))
                {
                    parsed.AllowCavalryReformWithoutCommander = d.AllowCavalryReformWithoutCommander;
                }
            }
            catch
            {
                // Ignore merge failures.
            }
        }

        /// <summary>
        /// Omitted Slice 11 keys deserialize to 0 / false — restore eligibility defaults when absent from JSON.
        /// </summary>
        private static void ApplyOmittedSlice11EligibilityDefaults(string json, CommanderConfig parsed)
        {
            if (parsed == null || string.IsNullOrWhiteSpace(json))
            {
                return;
            }

            try
            {
                using JsonDocument doc = JsonDocument.Parse(json);
                if (doc.RootElement.ValueKind != JsonValueKind.Object)
                {
                    return;
                }

                JsonElement root = doc.RootElement;
                CommanderConfig d = CommanderConfigDefaults.CreateDefault();
                if (!JsonHasPropertyIgnoreCase(root, nameof(CommanderConfig.BasicLineMinimumDiscipline)))
                {
                    parsed.BasicLineMinimumDiscipline = d.BasicLineMinimumDiscipline;
                }

                if (!JsonHasPropertyIgnoreCase(root, nameof(CommanderConfig.LooseMinimumDiscipline)))
                {
                    parsed.LooseMinimumDiscipline = d.LooseMinimumDiscipline;
                }

                if (!JsonHasPropertyIgnoreCase(root, nameof(CommanderConfig.ShieldWallMinimumDiscipline)))
                {
                    parsed.ShieldWallMinimumDiscipline = d.ShieldWallMinimumDiscipline;
                }

                if (!JsonHasPropertyIgnoreCase(root, nameof(CommanderConfig.SquareMinimumDiscipline)))
                {
                    parsed.SquareMinimumDiscipline = d.SquareMinimumDiscipline;
                }

                if (!JsonHasPropertyIgnoreCase(root, nameof(CommanderConfig.CircleMinimumDiscipline)))
                {
                    parsed.CircleMinimumDiscipline = d.CircleMinimumDiscipline;
                }

                if (!JsonHasPropertyIgnoreCase(root, nameof(CommanderConfig.AdvancedAdaptiveMinimumDiscipline)))
                {
                    parsed.AdvancedAdaptiveMinimumDiscipline = d.AdvancedAdaptiveMinimumDiscipline;
                }

                if (!JsonHasPropertyIgnoreCase(root, nameof(CommanderConfig.MinimumShieldRatioForShieldWall)))
                {
                    parsed.MinimumShieldRatioForShieldWall = d.MinimumShieldRatioForShieldWall;
                }

                if (!JsonHasPropertyIgnoreCase(root, nameof(CommanderConfig.MinimumPolearmOrShieldRatioForSquare)))
                {
                    parsed.MinimumPolearmOrShieldRatioForSquare = d.MinimumPolearmOrShieldRatioForSquare;
                }

                if (!JsonHasPropertyIgnoreCase(root, nameof(CommanderConfig.MinimumMountedRatioForMountedWide)))
                {
                    parsed.MinimumMountedRatioForMountedWide = d.MinimumMountedRatioForMountedWide;
                }

                if (!JsonHasPropertyIgnoreCase(root, nameof(CommanderConfig.MinimumHorseArcherRatioForHorseArcherLoose)))
                {
                    parsed.MinimumHorseArcherRatioForHorseArcherLoose = d.MinimumHorseArcherRatioForHorseArcherLoose;
                }

                if (!JsonHasPropertyIgnoreCase(root, nameof(CommanderConfig.EnableEligibilityDebug)))
                {
                    parsed.EnableEligibilityDebug = d.EnableEligibilityDebug;
                }
            }
            catch
            {
                // Ignore merge failures.
            }
        }

        /// <summary>
        /// Omitted Slice 15 keys deserialize to false / 0 — restore command router defaults when absent from JSON.
        /// </summary>
        private static void ApplyOmittedSlice16NativeCavalrySequenceDefaults(string json, CommanderConfig parsed)
        {
            if (parsed == null || string.IsNullOrWhiteSpace(json))
            {
                return;
            }

            try
            {
                using JsonDocument doc = JsonDocument.Parse(json);
                if (doc.RootElement.ValueKind != JsonValueKind.Object)
                {
                    return;
                }

                JsonElement root = doc.RootElement;
                CommanderConfig d = CommanderConfigDefaults.CreateDefault();
                if (!JsonHasPropertyIgnoreCase(root, nameof(CommanderConfig.EnableNativeCavalryChargeSequence)))
                {
                    parsed.EnableNativeCavalryChargeSequence = d.EnableNativeCavalryChargeSequence;
                }

                if (!JsonHasPropertyIgnoreCase(root, nameof(CommanderConfig.CavalryUseNativeForwardBeforeCharge)))
                {
                    parsed.CavalryUseNativeForwardBeforeCharge = d.CavalryUseNativeForwardBeforeCharge;
                }

                if (!JsonHasPropertyIgnoreCase(root, nameof(CommanderConfig.CavalryUseNativeChargeCommand)))
                {
                    parsed.CavalryUseNativeChargeCommand = d.CavalryUseNativeChargeCommand;
                }

                if (!JsonHasPropertyIgnoreCase(root, nameof(CommanderConfig.CavalryForwardToChargeDistance)))
                {
                    parsed.CavalryForwardToChargeDistance = d.CavalryForwardToChargeDistance;
                }

                if (!JsonHasPropertyIgnoreCase(root, nameof(CommanderConfig.EnableCavalrySequenceDebug)))
                {
                    parsed.EnableCavalrySequenceDebug = d.EnableCavalrySequenceDebug;
                }
            }
            catch
            {
                // Ignore merge failures.
            }
        }

        private static void ApplyOmittedSlice20DiagnosticsDefaults(string json, CommanderConfig parsed)
        {
            if (parsed == null || string.IsNullOrWhiteSpace(json))
            {
                return;
            }

            try
            {
                using JsonDocument doc = JsonDocument.Parse(json);
                if (doc.RootElement.ValueKind != JsonValueKind.Object)
                {
                    return;
                }

                JsonElement root = doc.RootElement;
                CommanderConfig d = CommanderConfigDefaults.CreateDefault();
                if (!JsonHasPropertyIgnoreCase(root, nameof(CommanderConfig.EnableDiagnostics)))
                {
                    parsed.EnableDiagnostics = d.EnableDiagnostics;
                }

                if (!JsonHasPropertyIgnoreCase(root, nameof(CommanderConfig.ShowDiagnosticsInCommanderModeOnly)))
                {
                    parsed.ShowDiagnosticsInCommanderModeOnly = d.ShowDiagnosticsInCommanderModeOnly;
                }

                if (!JsonHasPropertyIgnoreCase(root, nameof(CommanderConfig.DiagnosticsToggleKey)))
                {
                    parsed.DiagnosticsToggleKey = d.DiagnosticsToggleKey;
                }

                if (!JsonHasPropertyIgnoreCase(root, nameof(CommanderConfig.DiagnosticsRefreshIntervalSeconds)))
                {
                    parsed.DiagnosticsRefreshIntervalSeconds = d.DiagnosticsRefreshIntervalSeconds;
                }

                if (!JsonHasPropertyIgnoreCase(root, nameof(CommanderConfig.IncludeDoctrineScores)))
                {
                    parsed.IncludeDoctrineScores = d.IncludeDoctrineScores;
                }

                if (!JsonHasPropertyIgnoreCase(root, nameof(CommanderConfig.IncludeEligibility)))
                {
                    parsed.IncludeEligibility = d.IncludeEligibility;
                }

                if (!JsonHasPropertyIgnoreCase(root, nameof(CommanderConfig.IncludeRallyAbsorption)))
                {
                    parsed.IncludeRallyAbsorption = d.IncludeRallyAbsorption;
                }

                if (!JsonHasPropertyIgnoreCase(root, nameof(CommanderConfig.IncludeCavalrySequence)))
                {
                    parsed.IncludeCavalrySequence = d.IncludeCavalrySequence;
                }

                if (!JsonHasPropertyIgnoreCase(root, nameof(CommanderConfig.IncludeNativeOrderStatus)))
                {
                    parsed.IncludeNativeOrderStatus = d.IncludeNativeOrderStatus;
                }
            }
            catch
            {
                // Ignore merge failures.
            }
        }

        private static void ApplyOmittedSlice24PerformanceBudgetDefaults(string json, CommanderConfig parsed)
        {
            if (parsed == null || string.IsNullOrWhiteSpace(json))
            {
                return;
            }

            try
            {
                using JsonDocument doc = JsonDocument.Parse(json);
                if (doc.RootElement.ValueKind != JsonValueKind.Object)
                {
                    return;
                }

                JsonElement root = doc.RootElement;
                CommanderConfig d = CommanderConfigDefaults.CreateDefault();
                if (!JsonHasPropertyIgnoreCase(root, nameof(CommanderConfig.EnablePerformanceDiagnostics)))
                {
                    parsed.EnablePerformanceDiagnostics = d.EnablePerformanceDiagnostics;
                }

                if (!JsonHasPropertyIgnoreCase(root, nameof(CommanderConfig.WarnOnOverBudget)))
                {
                    parsed.WarnOnOverBudget = d.WarnOnOverBudget;
                }

                if (!JsonHasPropertyIgnoreCase(root, nameof(CommanderConfig.PerformanceWarningThrottleSeconds)))
                {
                    parsed.PerformanceWarningThrottleSeconds = d.PerformanceWarningThrottleSeconds;
                }

                if (!JsonHasPropertyIgnoreCase(root, nameof(CommanderConfig.TargetingIntervalSeconds)))
                {
                    parsed.TargetingIntervalSeconds = d.TargetingIntervalSeconds;
                }

                if (!JsonHasPropertyIgnoreCase(root, nameof(CommanderConfig.CommanderScanIntervalSeconds)))
                {
                    parsed.CommanderScanIntervalSeconds = d.CommanderScanIntervalSeconds;
                }

                if (!JsonHasPropertyIgnoreCase(root, nameof(CommanderConfig.EligibilityScanIntervalSeconds)))
                {
                    parsed.EligibilityScanIntervalSeconds = d.EligibilityScanIntervalSeconds;
                }

                if (!JsonHasPropertyIgnoreCase(root, nameof(CommanderConfig.RallyAbsorptionIntervalSeconds)))
                {
                    parsed.RallyAbsorptionIntervalSeconds = d.RallyAbsorptionIntervalSeconds;
                }

                if (!JsonHasPropertyIgnoreCase(root, nameof(CommanderConfig.CavalrySequenceIntervalSeconds)))
                {
                    parsed.CavalrySequenceIntervalSeconds = d.CavalrySequenceIntervalSeconds;
                }

                if (!JsonHasPropertyIgnoreCase(root, nameof(CommanderConfig.FeedbackTickIntervalSeconds)))
                {
                    parsed.FeedbackTickIntervalSeconds = d.FeedbackTickIntervalSeconds;
                }

                if (!JsonHasPropertyIgnoreCase(root, nameof(CommanderConfig.MarkerTickIntervalSeconds)))
                {
                    parsed.MarkerTickIntervalSeconds = d.MarkerTickIntervalSeconds;
                }

                if (!JsonHasPropertyIgnoreCase(root, nameof(CommanderConfig.DiagnosticsTickIntervalSeconds)))
                {
                    parsed.DiagnosticsTickIntervalSeconds = d.DiagnosticsTickIntervalSeconds;
                }

                if (!JsonHasPropertyIgnoreCase(root, nameof(CommanderConfig.ConfigReloadCheckIntervalSeconds)))
                {
                    parsed.ConfigReloadCheckIntervalSeconds = d.ConfigReloadCheckIntervalSeconds;
                }
            }
            catch
            {
                // Ignore merge failures.
            }
        }

        private static void ApplyOmittedSlice19CommandMarkerDefaults(string json, CommanderConfig parsed)
        {
            if (parsed == null || string.IsNullOrWhiteSpace(json))
            {
                return;
            }

            try
            {
                using JsonDocument doc = JsonDocument.Parse(json);
                if (doc.RootElement.ValueKind != JsonValueKind.Object)
                {
                    return;
                }

                JsonElement root = doc.RootElement;
                CommanderConfig d = CommanderConfigDefaults.CreateDefault();
                if (!JsonHasPropertyIgnoreCase(root, nameof(CommanderConfig.EnableCommandMarkers)))
                {
                    parsed.EnableCommandMarkers = d.EnableCommandMarkers;
                }

                if (!JsonHasPropertyIgnoreCase(root, nameof(CommanderConfig.EnableFallbackTextMarkers)))
                {
                    parsed.EnableFallbackTextMarkers = d.EnableFallbackTextMarkers;
                }

                if (!JsonHasPropertyIgnoreCase(root, nameof(CommanderConfig.DefaultMarkerLifetimeSeconds)))
                {
                    parsed.DefaultMarkerLifetimeSeconds = d.DefaultMarkerLifetimeSeconds;
                }

                if (!JsonHasPropertyIgnoreCase(root, nameof(CommanderConfig.ChargeMarkerLifetimeSeconds)))
                {
                    parsed.ChargeMarkerLifetimeSeconds = d.ChargeMarkerLifetimeSeconds;
                }

                if (!JsonHasPropertyIgnoreCase(root, nameof(CommanderConfig.ReformMarkerLifetimeSeconds)))
                {
                    parsed.ReformMarkerLifetimeSeconds = d.ReformMarkerLifetimeSeconds;
                }

                if (!JsonHasPropertyIgnoreCase(root, nameof(CommanderConfig.MarkerRefreshThrottleSeconds)))
                {
                    parsed.MarkerRefreshThrottleSeconds = d.MarkerRefreshThrottleSeconds;
                }
            }
            catch
            {
                // Ignore merge failures.
            }
        }

        /// <summary>
        /// Omitted Slice 14 keys deserialize to false — restore per-primitive allow defaults when absent from JSON.
        /// </summary>
        private static void ApplyOmittedSlice14NativeOrderDefaults(string json, CommanderConfig parsed)
        {
            if (parsed == null || string.IsNullOrWhiteSpace(json))
            {
                return;
            }

            try
            {
                using JsonDocument doc = JsonDocument.Parse(json);
                if (doc.RootElement.ValueKind != JsonValueKind.Object)
                {
                    return;
                }

                JsonElement root = doc.RootElement;
                CommanderConfig d = CommanderConfigDefaults.CreateDefault();
                if (!JsonHasPropertyIgnoreCase(root, nameof(CommanderConfig.EnableNativeOrderExecution)))
                {
                    parsed.EnableNativeOrderExecution = d.EnableNativeOrderExecution;
                }

                if (!JsonHasPropertyIgnoreCase(root, nameof(CommanderConfig.AllowNativeAdvanceOrMove)))
                {
                    parsed.AllowNativeAdvanceOrMove = d.AllowNativeAdvanceOrMove;
                }

                if (!JsonHasPropertyIgnoreCase(root, nameof(CommanderConfig.AllowNativeCharge)))
                {
                    parsed.AllowNativeCharge = d.AllowNativeCharge;
                }

                if (!JsonHasPropertyIgnoreCase(root, nameof(CommanderConfig.AllowNativeHold)))
                {
                    parsed.AllowNativeHold = d.AllowNativeHold;
                }

                if (!JsonHasPropertyIgnoreCase(root, nameof(CommanderConfig.AllowNativeReform)))
                {
                    parsed.AllowNativeReform = d.AllowNativeReform;
                }

                if (!JsonHasPropertyIgnoreCase(root, nameof(CommanderConfig.AllowNativeFollowCommander)))
                {
                    parsed.AllowNativeFollowCommander = d.AllowNativeFollowCommander;
                }

                if (!JsonHasPropertyIgnoreCase(root, nameof(CommanderConfig.AllowNativeStop)))
                {
                    parsed.AllowNativeStop = d.AllowNativeStop;
                }

                if (!JsonHasPropertyIgnoreCase(root, nameof(CommanderConfig.EnableNativeOrderDebug)))
                {
                    parsed.EnableNativeOrderDebug = d.EnableNativeOrderDebug;
                }
            }
            catch
            {
                // Ignore merge failures.
            }
        }

        private static void ApplyOmittedSlice15CommandRouterDefaults(string json, CommanderConfig parsed)
        {
            if (parsed == null || string.IsNullOrWhiteSpace(json))
            {
                return;
            }

            try
            {
                using JsonDocument doc = JsonDocument.Parse(json);
                if (doc.RootElement.ValueKind != JsonValueKind.Object)
                {
                    return;
                }

                JsonElement root = doc.RootElement;
                CommanderConfig d = CommanderConfigDefaults.CreateDefault();
                if (!JsonHasPropertyIgnoreCase(root, nameof(CommanderConfig.EnableCommandRouter)))
                {
                    parsed.EnableCommandRouter = d.EnableCommandRouter;
                }

                if (!JsonHasPropertyIgnoreCase(root, nameof(CommanderConfig.EnableCommandValidationDebug)))
                {
                    parsed.EnableCommandValidationDebug = d.EnableCommandValidationDebug;
                }

                if (!JsonHasPropertyIgnoreCase(root, nameof(CommanderConfig.AllowBasicChargeWithoutAdvancedDoctrine)))
                {
                    parsed.AllowBasicChargeWithoutAdvancedDoctrine = d.AllowBasicChargeWithoutAdvancedDoctrine;
                }

                if (!JsonHasPropertyIgnoreCase(root, nameof(CommanderConfig.AllowNoCommanderBasicHold)))
                {
                    parsed.AllowNoCommanderBasicHold = d.AllowNoCommanderBasicHold;
                }

                if (!JsonHasPropertyIgnoreCase(root, nameof(CommanderConfig.AllowNoCommanderBasicFollow)))
                {
                    parsed.AllowNoCommanderBasicFollow = d.AllowNoCommanderBasicFollow;
                }

                if (!JsonHasPropertyIgnoreCase(root, nameof(CommanderConfig.BlockAdvancedCommandsWithoutCommander)))
                {
                    parsed.BlockAdvancedCommandsWithoutCommander = d.BlockAdvancedCommandsWithoutCommander;
                }

                if (!JsonHasPropertyIgnoreCase(root, nameof(CommanderConfig.EnableNativePrimitiveOrderExecution)))
                {
                    parsed.EnableNativePrimitiveOrderExecution = d.EnableNativePrimitiveOrderExecution;
                }

                if (!JsonHasPropertyIgnoreCase(root, nameof(CommanderConfig.CommandValidationDebugLogIntervalSeconds)))
                {
                    parsed.CommandValidationDebugLogIntervalSeconds = d.CommandValidationDebugLogIntervalSeconds;
                }
            }
            catch
            {
                // Ignore merge failures.
            }
        }

        private static string ResolveConfigPath()
        {
            string gameRoot = BasePath.Name;
            string moduleRoot = Path.Combine(gameRoot, "Modules", ModConstants.ModuleId);
            return Path.Combine(moduleRoot, CommanderConfigDefaults.RelativeConfigPath);
        }

        private static CommanderConfigValidationResult FinalizeLoadedConfig(
            string path,
            string rawJson,
            CommanderConfig parsed,
            CommanderConfig defaults)
        {
            CommanderConfigMigration.MigrationOutcome migration = CommanderConfigMigration.Apply(rawJson, parsed, defaults);
            CommanderConfigValidationResult validation = CommanderConfigValidator.Validate(parsed, defaults);
            var mergedWarnings = new List<string>(migration.Warnings.Count + validation.Warnings.Count);
            mergedWarnings.AddRange(migration.Warnings);
            mergedWarnings.AddRange(validation.Warnings);
            bool requiresRewrite = validation.RequiresRewrite || migration.NeedsPersist;
            var combined = new CommanderConfigValidationResult(
                validation.IsValid,
                validation.UsedFallbacks,
                requiresRewrite,
                mergedWarnings,
                validation.Errors,
                validation.SanitizedConfig);

            LogValidationMessages(combined);
            TryRewriteSanitizedConfig(path, combined);
            return combined;
        }

        private static void LogValidationMessages(CommanderConfigValidationResult result)
        {
            const int maxLines = 24;
            int i = 0;
            foreach (string w in result.Warnings)
            {
                if (i++ >= maxLines)
                {
                    ModLogger.Warn($"{ModConstants.ModuleId}: config validation: additional warnings omitted ({result.Warnings.Count - maxLines}).");
                    break;
                }

                ModLogger.Warn($"{ModConstants.ModuleId}: config validation: {w}");
            }

            foreach (string e in result.Errors)
            {
                ModLogger.Warn($"{ModConstants.ModuleId}: config validation error: {e}");
            }
        }

        private static void TryRewriteSanitizedConfig(string path, CommanderConfigValidationResult result)
        {
            if (!result.RequiresRewrite || string.IsNullOrEmpty(path))
            {
                return;
            }

            try
            {
                WriteConfig(path, result.SanitizedConfig);
                ModLogger.LogDebug($"{ModConstants.ModuleId}: wrote sanitized commander_config.json ({path})");
            }
            catch (Exception ex)
            {
                ModLogger.LogWarningOnce(
                    "commander_config_rewrite",
                    $"{ModConstants.ModuleId}: could not rewrite sanitized config ({ex.Message}).");
            }
        }

        private static void WriteConfig(string path, CommanderConfig config)
        {
            string json = JsonSerializer.Serialize(config, WriteJsonOptions);
            File.WriteAllText(path, json);
        }

        /// <summary>
        /// Fail-closed preflight for attaching <see cref="Bannerlord.RTSCameraLite.Mission.CommanderMissionView"/>: returns true only when
        /// <c>commander_config.json</c> is readable and explicitly sets <see cref="CommanderConfig.EnableMissionRuntimeHooks"/> to true.
        /// Missing file, invalid JSON, unreadable path, or any exception yields false (no exceptions propagate).
        /// </summary>
        public static bool TryReadMissionRuntimeHooksEnabledFailClosed()
        {
            try
            {
                return CommanderRuntimeHookGate.IsMissionRuntimeHooksEnabledSafe();
            }
            catch
            {
                return false;
            }
        }
    }
}
