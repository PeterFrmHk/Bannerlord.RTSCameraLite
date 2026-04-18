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
