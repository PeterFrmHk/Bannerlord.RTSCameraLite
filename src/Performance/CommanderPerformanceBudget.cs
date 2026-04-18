using System;
using Bannerlord.RTSCameraLite.Config;

namespace Bannerlord.RTSCameraLite.Performance
{
    /// <summary>
    /// Central cadence for throttled mission updates (Slice 24). Intervals are read from <see cref="CommanderConfig"/>.
    /// </summary>
    public sealed class CommanderPerformanceBudget
    {
        public const float DefaultTargetingIntervalSeconds = 0.25f;

        public const float DefaultCommanderScanIntervalSeconds = 3.0f;

        public const float DefaultDoctrineScanIntervalSeconds = 3.0f;

        public const float DefaultEligibilityScanIntervalSeconds = 3.0f;

        public const float DefaultRallyAbsorptionIntervalSeconds = 3.0f;

        public const float DefaultCavalrySequenceIntervalSeconds = 0.25f;

        public const float DefaultFeedbackTickIntervalSeconds = 0.1f;

        public const float DefaultMarkerTickIntervalSeconds = 0.1f;

        public const float DefaultDiagnosticsIntervalSeconds = 1.0f;

        public const float DefaultConfigReloadCheckIntervalSeconds = 5.0f;

        public CommanderPerformanceBudget(
            float targetingIntervalSeconds,
            float commanderScanIntervalSeconds,
            float doctrineScanIntervalSeconds,
            float eligibilityScanIntervalSeconds,
            float rallyAbsorptionIntervalSeconds,
            float cavalrySequenceIntervalSeconds,
            float feedbackTickIntervalSeconds,
            float markerTickIntervalSeconds,
            float diagnosticsIntervalSeconds,
            float configReloadCheckIntervalSeconds)
        {
            TargetingIntervalSeconds = targetingIntervalSeconds;
            CommanderScanIntervalSeconds = commanderScanIntervalSeconds;
            DoctrineScanIntervalSeconds = doctrineScanIntervalSeconds;
            EligibilityScanIntervalSeconds = eligibilityScanIntervalSeconds;
            RallyAbsorptionIntervalSeconds = rallyAbsorptionIntervalSeconds;
            CavalrySequenceIntervalSeconds = cavalrySequenceIntervalSeconds;
            FeedbackTickIntervalSeconds = feedbackTickIntervalSeconds;
            MarkerTickIntervalSeconds = markerTickIntervalSeconds;
            DiagnosticsIntervalSeconds = diagnosticsIntervalSeconds;
            ConfigReloadCheckIntervalSeconds = configReloadCheckIntervalSeconds;
        }

        public float TargetingIntervalSeconds { get; }

        public float CommanderScanIntervalSeconds { get; }

        public float DoctrineScanIntervalSeconds { get; }

        public float EligibilityScanIntervalSeconds { get; }

        public float RallyAbsorptionIntervalSeconds { get; }

        public float CavalrySequenceIntervalSeconds { get; }

        public float FeedbackTickIntervalSeconds { get; }

        public float MarkerTickIntervalSeconds { get; }

        public float DiagnosticsIntervalSeconds { get; }

        public float ConfigReloadCheckIntervalSeconds { get; }

        public static CommanderPerformanceBudget FromCommanderConfig(CommanderConfig c)
        {
            if (c == null)
            {
                return CreateDefaults();
            }

            float rally = c.RallyAbsorptionIntervalSeconds > 0f
                ? c.RallyAbsorptionIntervalSeconds
                : Math.Max(0.1f, c.RallyScanIntervalSeconds);

            float diagTick = c.DiagnosticsTickIntervalSeconds > 0f
                ? c.DiagnosticsTickIntervalSeconds
                : System.Math.Max(0.1f, c.DiagnosticsRefreshIntervalSeconds);

            return new CommanderPerformanceBudget(
                ClampPos(c.TargetingIntervalSeconds, DefaultTargetingIntervalSeconds),
                ClampPos(c.CommanderScanIntervalSeconds, DefaultCommanderScanIntervalSeconds),
                ClampPos(c.DoctrineScanIntervalSeconds, DefaultDoctrineScanIntervalSeconds),
                ClampPos(c.EligibilityScanIntervalSeconds, DefaultEligibilityScanIntervalSeconds),
                ClampPos(rally, DefaultRallyAbsorptionIntervalSeconds),
                ClampPos(c.CavalrySequenceIntervalSeconds, DefaultCavalrySequenceIntervalSeconds),
                ClampPos(c.FeedbackTickIntervalSeconds, DefaultFeedbackTickIntervalSeconds),
                ClampPos(c.MarkerTickIntervalSeconds, DefaultMarkerTickIntervalSeconds),
                ClampPos(diagTick, DefaultDiagnosticsIntervalSeconds),
                ClampPos(c.ConfigReloadCheckIntervalSeconds, DefaultConfigReloadCheckIntervalSeconds));
        }

        public static CommanderPerformanceBudget CreateDefaults()
        {
            return new CommanderPerformanceBudget(
                DefaultTargetingIntervalSeconds,
                DefaultCommanderScanIntervalSeconds,
                DefaultDoctrineScanIntervalSeconds,
                DefaultEligibilityScanIntervalSeconds,
                DefaultRallyAbsorptionIntervalSeconds,
                DefaultCavalrySequenceIntervalSeconds,
                DefaultFeedbackTickIntervalSeconds,
                DefaultMarkerTickIntervalSeconds,
                DefaultDiagnosticsIntervalSeconds,
                DefaultConfigReloadCheckIntervalSeconds);
        }

        public float GetInterval(UpdateBudgetCategory category)
        {
            switch (category)
            {
                case UpdateBudgetCategory.Targeting:
                    return TargetingIntervalSeconds;
                case UpdateBudgetCategory.CommanderScan:
                    return CommanderScanIntervalSeconds;
                case UpdateBudgetCategory.DoctrineScan:
                    return DoctrineScanIntervalSeconds;
                case UpdateBudgetCategory.EligibilityScan:
                    return EligibilityScanIntervalSeconds;
                case UpdateBudgetCategory.RallyAbsorptionScan:
                    return RallyAbsorptionIntervalSeconds;
                case UpdateBudgetCategory.CavalrySequenceTick:
                    return CavalrySequenceIntervalSeconds;
                case UpdateBudgetCategory.FeedbackTick:
                    return FeedbackTickIntervalSeconds;
                case UpdateBudgetCategory.MarkerTick:
                    return MarkerTickIntervalSeconds;
                case UpdateBudgetCategory.DiagnosticsTick:
                    return DiagnosticsIntervalSeconds;
                case UpdateBudgetCategory.ConfigReloadCheck:
                    return ConfigReloadCheckIntervalSeconds;
                default:
                    return 0.25f;
            }
        }

        private static float ClampPos(float value, float fallback)
        {
            if (float.IsNaN(value) || float.IsInfinity(value) || value <= 0f)
            {
                return fallback;
            }

            return value;
        }
    }
}
