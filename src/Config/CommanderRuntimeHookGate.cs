using System;
using System.IO;
using System.Text.RegularExpressions;
using Bannerlord.RTSCameraLite.Core;
using TaleWorlds.Library;

namespace Bannerlord.RTSCameraLite.Config
{
    /// <summary>
    /// Minimal fail-closed preflight for mission runtime attachment. Avoids JSON serializer dependency during mission hook registration.
    /// </summary>
    public static class CommanderRuntimeHookGate
    {
        private static readonly Regex EnableMissionRuntimeHooksRegex = new Regex(
            "\"EnableMissionRuntimeHooks\"\\s*:\\s*(true|false)",
            RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);

        public static bool IsMissionRuntimeHooksEnabledSafe()
        {
            try
            {
                string configPath = ResolveConfigPathSafe();
                if (string.IsNullOrEmpty(configPath) || !File.Exists(configPath))
                {
                    return false;
                }

                string raw = File.ReadAllText(configPath);
                return TryParseEnableMissionRuntimeHooks(raw);
            }
            catch (Exception ex)
            {
                ModLogger.LogWarningOnce(
                    "mission_runtime_hook_preflight",
                    $"{ModConstants.ModuleId}: runtime hook preflight failed ({ex.Message}); mission runtime stays dormant.");
                return false;
            }
        }

        public static bool TryParseEnableMissionRuntimeHooks(string raw)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(raw))
                {
                    return false;
                }

                Match match = EnableMissionRuntimeHooksRegex.Match(raw);
                if (!match.Success)
                {
                    return false;
                }

                return string.Equals(match.Groups[1].Value, "true", StringComparison.OrdinalIgnoreCase);
            }
            catch
            {
                return false;
            }
        }

        private static string ResolveConfigPathSafe()
        {
            string gameRoot = BasePath.Name;
            if (string.IsNullOrWhiteSpace(gameRoot))
            {
                return string.Empty;
            }

            string moduleRoot = Path.Combine(gameRoot, "Modules", ModConstants.ModuleId);
            return Path.Combine(moduleRoot, CommanderConfigDefaults.RelativeConfigPath);
        }
    }
}
