using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Text.Json;
using TaleWorlds.InputSystem;

namespace Bannerlord.RTSCameraLite.Config
{
    /// <summary>
    /// Resolves module config path, loads JSON, repairs invalid values, and materializes key bindings.
    /// Never throws to callers; failures yield defaults and diagnostic text.
    /// </summary>
    internal static class ConfigService
    {
        private const string ConfigRelativeDir = "config";
        private const string ConfigFileName = "rts_camera_lite.json";

        private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            ReadCommentHandling = JsonCommentHandling.Skip,
            AllowTrailingCommas = true
        };

        public static string TryGetModuleRootFromAssembly()
        {
            try
            {
                string location = Assembly.GetExecutingAssembly().Location;
                if (string.IsNullOrEmpty(location))
                {
                    return null;
                }

                string binDirectory = Path.GetDirectoryName(location);
                if (string.IsNullOrEmpty(binDirectory))
                {
                    return null;
                }

                return Path.GetFullPath(Path.Combine(binDirectory, "..", ".."));
            }
            catch
            {
                return null;
            }
        }

        public static string GetConfigFilePath(string moduleRoot)
        {
            return Path.Combine(moduleRoot, ConfigRelativeDir, ConfigFileName);
        }

        /// <summary>
        /// Loads config from disk or creates default file. Returns effective config and human-readable load notes.
        /// </summary>
        public static RTSCameraConfig LoadOrCreate(string moduleRoot, out string diagnostics)
        {
            List<string> notes = new List<string>();
            RTSCameraConfig defaults = ConfigDefaults.CreateDefault();

            if (string.IsNullOrWhiteSpace(moduleRoot))
            {
                notes.Add("Config path unavailable; using built-in defaults.");
                RTSCameraConfig noRoot = MergeAndSanitize(defaults, defaults, notes);
                diagnostics = string.Join(" ", notes);
                return noRoot;
            }

            string configPath = GetConfigFilePath(moduleRoot);
            try
            {
                string directory = Path.GetDirectoryName(configPath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }
            }
            catch (Exception ex)
            {
                notes.Add($"Could not ensure config directory: {ex.Message}");
                RTSCameraConfig dirFail = MergeAndSanitize(defaults, defaults, notes);
                diagnostics = string.Join(" ", notes);
                return dirFail;
            }

            if (!File.Exists(configPath))
            {
                try
                {
                    WriteConfigFile(configPath, defaults);
                    notes.Add($"Created default config at {configPath}.");
                }
                catch (Exception ex)
                {
                    notes.Add($"Missing config and could not write default file: {ex.Message}");
                }

                RTSCameraConfig missingFile = MergeAndSanitize(defaults, defaults, notes);
                diagnostics = string.Join(" ", notes);
                return missingFile;
            }

            RTSCameraConfig loaded = null;
            try
            {
                string json = File.ReadAllText(configPath);
                loaded = JsonSerializer.Deserialize<RTSCameraConfig>(json, JsonOptions);
            }
            catch (Exception ex)
            {
                notes.Add($"Config read/parse failed; using defaults. ({ex.Message})");
                RTSCameraConfig parseFail = MergeAndSanitize(defaults, defaults, notes);
                diagnostics = string.Join(" ", notes);
                return parseFail;
            }

            if (loaded == null)
            {
                notes.Add("Config deserialized to null; using defaults.");
                RTSCameraConfig nullCfg = MergeAndSanitize(defaults, defaults, notes);
                diagnostics = string.Join(" ", notes);
                return nullCfg;
            }

            RTSCameraConfig merged = MergeAndSanitize(loaded, defaults, notes);
            diagnostics = notes.Count > 0 ? string.Join(" ", notes) : "Config loaded OK.";
            return merged;
        }

        public static CameraKeyBindings BuildKeyBindings(RTSCameraConfig effective, RTSCameraConfig defaults)
        {
            return new CameraKeyBindings(
                toggle: ResolveKey(effective.ToggleKey, defaults.ToggleKey),
                moveForward: ResolveKey(effective.MoveForwardKey, defaults.MoveForwardKey),
                moveBack: ResolveKey(effective.MoveBackKey, defaults.MoveBackKey),
                moveLeft: ResolveKey(effective.MoveLeftKey, defaults.MoveLeftKey),
                moveRight: ResolveKey(effective.MoveRightKey, defaults.MoveRightKey),
                rotateLeft: ResolveKey(effective.RotateLeftKey, defaults.RotateLeftKey),
                rotateRight: ResolveKey(effective.RotateRightKey, defaults.RotateRightKey),
                fastMove: ResolveKey(effective.FastMoveKey, defaults.FastMoveKey),
                zoomIn: ResolveKey(effective.ZoomInKey, defaults.ZoomInKey),
                zoomOut: ResolveKey(effective.ZoomOutKey, defaults.ZoomOutKey),
                nextFormation: ResolveKey(effective.NextFormationKey, defaults.NextFormationKey),
                previousFormation: ResolveKey(effective.PreviousFormationKey, defaults.PreviousFormationKey),
                focusSelectedFormation: ResolveKey(effective.FocusSelectedFormationKey, defaults.FocusSelectedFormationKey));
        }

        private static void WriteConfigFile(string path, RTSCameraConfig config)
        {
            string json = JsonSerializer.Serialize(config, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(path, json, Encoding.UTF8);
        }

        private static RTSCameraConfig MergeAndSanitize(RTSCameraConfig loaded, RTSCameraConfig defaults, List<string> notes)
        {
            RTSCameraConfig d = ConfigDefaults.CreateDefault();

            d.ToggleKey = CoerceStringKey(loaded.ToggleKey, defaults.ToggleKey, nameof(RTSCameraConfig.ToggleKey), notes);
            d.MoveForwardKey = CoerceStringKey(loaded.MoveForwardKey, defaults.MoveForwardKey, nameof(RTSCameraConfig.MoveForwardKey), notes);
            d.MoveBackKey = CoerceStringKey(loaded.MoveBackKey, defaults.MoveBackKey, nameof(RTSCameraConfig.MoveBackKey), notes);
            d.MoveLeftKey = CoerceStringKey(loaded.MoveLeftKey, defaults.MoveLeftKey, nameof(RTSCameraConfig.MoveLeftKey), notes);
            d.MoveRightKey = CoerceStringKey(loaded.MoveRightKey, defaults.MoveRightKey, nameof(RTSCameraConfig.MoveRightKey), notes);
            d.RotateLeftKey = CoerceStringKey(loaded.RotateLeftKey, defaults.RotateLeftKey, nameof(RTSCameraConfig.RotateLeftKey), notes);
            d.RotateRightKey = CoerceStringKey(loaded.RotateRightKey, defaults.RotateRightKey, nameof(RTSCameraConfig.RotateRightKey), notes);
            d.FastMoveKey = CoerceStringKey(loaded.FastMoveKey, defaults.FastMoveKey, nameof(RTSCameraConfig.FastMoveKey), notes);
            d.ZoomInKey = CoerceStringKey(loaded.ZoomInKey, defaults.ZoomInKey, nameof(RTSCameraConfig.ZoomInKey), notes);
            d.ZoomOutKey = CoerceStringKey(loaded.ZoomOutKey, defaults.ZoomOutKey, nameof(RTSCameraConfig.ZoomOutKey), notes);
            d.NextFormationKey = CoerceStringKey(loaded.NextFormationKey, defaults.NextFormationKey, nameof(RTSCameraConfig.NextFormationKey), notes);
            d.PreviousFormationKey = CoerceStringKey(loaded.PreviousFormationKey, defaults.PreviousFormationKey, nameof(RTSCameraConfig.PreviousFormationKey), notes);
            d.FocusSelectedFormationKey = CoerceStringKey(loaded.FocusSelectedFormationKey, defaults.FocusSelectedFormationKey, nameof(RTSCameraConfig.FocusSelectedFormationKey), notes);

            d.MoveSpeed = CoercePositiveFloat(loaded.MoveSpeed, defaults.MoveSpeed, 0.1f, 200f, nameof(RTSCameraConfig.MoveSpeed), notes);
            d.FastMoveMultiplier = CoercePositiveFloat(loaded.FastMoveMultiplier, defaults.FastMoveMultiplier, 1f, 10f, nameof(RTSCameraConfig.FastMoveMultiplier), notes);
            d.RotationSpeedDegrees = CoercePositiveFloat(loaded.RotationSpeedDegrees, defaults.RotationSpeedDegrees, 1f, 720f, nameof(RTSCameraConfig.RotationSpeedDegrees), notes);
            d.ZoomSpeed = CoercePositiveFloat(loaded.ZoomSpeed, defaults.ZoomSpeed, 0f, 50f, nameof(RTSCameraConfig.ZoomSpeed), notes);

            float minH = CoercePositiveFloat(loaded.MinHeight, defaults.MinHeight, 1f, 500f, nameof(RTSCameraConfig.MinHeight), notes);
            float maxH = CoercePositiveFloat(loaded.MaxHeight, defaults.MaxHeight, 1f, 500f, nameof(RTSCameraConfig.MaxHeight), notes);
            if (minH >= maxH)
            {
                notes.Add("MinHeight >= MaxHeight; reset height bounds to defaults.");
                minH = defaults.MinHeight;
                maxH = defaults.MaxHeight;
            }

            d.MinHeight = minH;
            d.MaxHeight = maxH;
            d.DefaultHeight = CoercePositiveFloat(loaded.DefaultHeight, defaults.DefaultHeight, d.MinHeight, d.MaxHeight, nameof(RTSCameraConfig.DefaultHeight), notes);
            d.DefaultPitch = CoerceFloatRange(loaded.DefaultPitch, defaults.DefaultPitch, 0f, 89f, nameof(RTSCameraConfig.DefaultPitch), notes);

            return d;
        }

        private static string CoerceStringKey(string value, string fallback, string field, List<string> notes)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                notes.Add($"{field} empty; using default key name.");
                return fallback;
            }

            if (!TryParseKeyName(value, out _))
            {
                notes.Add($"{field} invalid key '{value}'; using default.");
                return fallback;
            }

            return value.Trim();
        }

        private static float CoercePositiveFloat(float value, float fallback, float min, float max, string field, List<string> notes)
        {
            if (float.IsNaN(value) || float.IsInfinity(value))
            {
                notes.Add($"{field} was NaN/Infinity; using default.");
                return Clamp(fallback, min, max);
            }

            if (value < min || value > max)
            {
                notes.Add($"{field} out of range; clamped.");
                return Clamp(value, min, max);
            }

            return value;
        }

        private static float CoerceFloatRange(float value, float fallback, float min, float max, string field, List<string> notes)
        {
            if (float.IsNaN(value) || float.IsInfinity(value))
            {
                notes.Add($"{field} was NaN/Infinity; using default.");
                return Clamp(fallback, min, max);
            }

            if (value < min || value > max)
            {
                notes.Add($"{field} out of range; clamped.");
                return Clamp(value, min, max);
            }

            return value;
        }

        private static float Clamp(float value, float min, float max)
        {
            if (value < min)
            {
                return min;
            }

            if (value > max)
            {
                return max;
            }

            return value;
        }

        internal static bool TryParseKeyName(string name, out InputKey key)
        {
            key = default;
            if (string.IsNullOrWhiteSpace(name))
            {
                return false;
            }

            string trimmed = name.Trim();
            return Enum.TryParse(trimmed, ignoreCase: true, out key);
        }

        private static InputKey ResolveKey(string configured, string fallbackName)
        {
            if (TryParseKeyName(configured, out InputKey k))
            {
                return k;
            }

            if (TryParseKeyName(fallbackName, out InputKey fb))
            {
                return fb;
            }

            TryParseKeyName("F10", out InputKey panic);
            return panic;
        }

        internal readonly struct CameraKeyBindings
        {
            public CameraKeyBindings(
                InputKey toggle,
                InputKey moveForward,
                InputKey moveBack,
                InputKey moveLeft,
                InputKey moveRight,
                InputKey rotateLeft,
                InputKey rotateRight,
                InputKey fastMove,
                InputKey zoomIn,
                InputKey zoomOut,
                InputKey nextFormation,
                InputKey previousFormation,
                InputKey focusSelectedFormation)
            {
                Toggle = toggle;
                MoveForward = moveForward;
                MoveBack = moveBack;
                MoveLeft = moveLeft;
                MoveRight = moveRight;
                RotateLeft = rotateLeft;
                RotateRight = rotateRight;
                FastMove = fastMove;
                ZoomIn = zoomIn;
                ZoomOut = zoomOut;
                NextFormation = nextFormation;
                PreviousFormation = previousFormation;
                FocusSelectedFormation = focusSelectedFormation;
            }

            public InputKey Toggle { get; }

            public InputKey MoveForward { get; }

            public InputKey MoveBack { get; }

            public InputKey MoveLeft { get; }

            public InputKey MoveRight { get; }

            public InputKey RotateLeft { get; }

            public InputKey RotateRight { get; }

            public InputKey FastMove { get; }

            public InputKey ZoomIn { get; }

            public InputKey ZoomOut { get; }

            public InputKey NextFormation { get; }

            public InputKey PreviousFormation { get; }

            public InputKey FocusSelectedFormation { get; }
        }
    }
}
