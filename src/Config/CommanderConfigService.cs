using System;
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
                    WriteConfig(configPath, defaults);
                    ModLogger.LogDebug($"{ModConstants.ModuleId}: created default {configPath}");
                    return new ConfigLoadResult(
                        loaded: true,
                        usedDefaults: false,
                        createdDefaultFile: true,
                        message: "Created default commander_config.json",
                        defaults);
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
                CommanderConfigDefaults.HarmonizeLegacyDetectionFields(parsed);
                ApplyOmittedSlice9AnchorDefaults(json, parsed);
                ApplyOmittedSlice10DoctrineDefaults(json, parsed);
                CommanderConfigDefaults.HarmonizeLegacyDoctrineFields(parsed);
                ApplyOmittedSlice11EligibilityDefaults(json, parsed);
                CommanderConfigDefaults.HarmonizeLegacyEligibilityFields(parsed);

                return new ConfigLoadResult(
                    loaded: true,
                    usedDefaults: false,
                    createdDefaultFile: false,
                    message: "OK",
                    parsed);
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
        /// Omitted Slice 11 keys deserialize to 0 / false — restore defaults when absent from JSON.
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

        private static string ResolveConfigPath()
        {
            string gameRoot = BasePath.Name;
            string moduleRoot = Path.Combine(gameRoot, "Modules", ModConstants.ModuleId);
            return Path.Combine(moduleRoot, CommanderConfigDefaults.RelativeConfigPath);
        }

        private static void WriteConfig(string path, CommanderConfig config)
        {
            string json = JsonSerializer.Serialize(config, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(path, json);
        }
    }
}
