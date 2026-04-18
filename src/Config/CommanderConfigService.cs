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
