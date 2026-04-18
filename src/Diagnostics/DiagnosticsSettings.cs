using Bannerlord.RTSCameraLite.Config;

namespace Bannerlord.RTSCameraLite.Diagnostics
{
    /// <summary>
    /// Portfolio / capture diagnostics (Slice 20 — text only, no HUD overlay).
    /// </summary>
    public sealed class DiagnosticsSettings
    {
        public bool EnableDiagnostics { get; set; }

        public bool ShowDiagnosticsInCommanderModeOnly { get; set; }

        public string DiagnosticsToggleKey { get; set; } = "F9";

        public float DiagnosticsRefreshIntervalSeconds { get; set; }

        public bool IncludeDoctrineScores { get; set; }

        public bool IncludeEligibility { get; set; }

        public bool IncludeRallyAbsorption { get; set; }

        public bool IncludeCavalrySequence { get; set; }

        public bool IncludeNativeOrderStatus { get; set; }

        public static DiagnosticsSettings FromCommanderConfig(CommanderConfig config)
        {
            CommanderConfig c = config ?? CommanderConfigDefaults.CreateDefault();
            return new DiagnosticsSettings
            {
                EnableDiagnostics = c.EnableDiagnostics,
                ShowDiagnosticsInCommanderModeOnly = c.ShowDiagnosticsInCommanderModeOnly,
                DiagnosticsToggleKey = string.IsNullOrWhiteSpace(c.DiagnosticsToggleKey)
                    ? "F9"
                    : c.DiagnosticsToggleKey,
                DiagnosticsRefreshIntervalSeconds = System.Math.Max(0.05f, c.DiagnosticsRefreshIntervalSeconds),
                IncludeDoctrineScores = c.IncludeDoctrineScores,
                IncludeEligibility = c.IncludeEligibility,
                IncludeRallyAbsorption = c.IncludeRallyAbsorption,
                IncludeCavalrySequence = c.IncludeCavalrySequence,
                IncludeNativeOrderStatus = c.IncludeNativeOrderStatus
            };
        }
    }
}
