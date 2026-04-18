using System.Text;
using Bannerlord.RTSCameraLite.Config;
using Bannerlord.RTSCameraLite.Core;

namespace Bannerlord.RTSCameraLite.Performance
{
    /// <summary>
    /// Slice 24 — optional, throttled reporting of scan cadence and over-budget work (debug only).
    /// </summary>
    public sealed class PerformanceDiagnosticsService
    {
        private readonly ThrottledUpdateGate _gate;
        private CommanderConfig _config;
        private float _warnAccumSeconds;

        public PerformanceDiagnosticsService(CommanderConfig config, ThrottledUpdateGate gate)
        {
            _config = config ?? CommanderConfigDefaults.CreateDefault();
            _gate = gate;
        }

        public void ApplyConfig(CommanderConfig config)
        {
            _config = config ?? CommanderConfigDefaults.CreateDefault();
        }

        /// <param name="missionTimeSeconds">Monotonic mission clock (seconds).</param>
        /// <param name="dt">Frame delta for warning throttle accumulation.</param>
        public void Tick(float missionTimeSeconds, float dt)
        {
            if (_config == null || !_config.EnablePerformanceDiagnostics || _gate == null)
            {
                return;
            }

            if (_gate.ShouldRun(UpdateBudgetCategory.ConfigReloadCheck, missionTimeSeconds))
            {
                _gate.MarkRun(UpdateBudgetCategory.ConfigReloadCheck, missionTimeSeconds, -1f);
                string summary = BuildScanRateSummary();
                if (!string.IsNullOrEmpty(summary))
                {
                    ModLogger.LogDebug($"{ModConstants.ModuleId}: perf budget — {summary}");
                }
            }

            TryWarnOverBudget(dt);
        }

        /// <summary>Emits at most one warning per throttle window when any category reports over budget.</summary>
        public void TryWarnOverBudget(float dt)
        {
            if (_config == null || !_config.WarnOnOverBudget || _gate == null)
            {
                return;
            }

            float throttle = System.Math.Max(0.5f, _config.PerformanceWarningThrottleSeconds);
            _warnAccumSeconds += dt;
            if (_warnAccumSeconds < throttle)
            {
                return;
            }

            _warnAccumSeconds = 0f;
            foreach (UpdateBudgetCategory cat in System.Enum.GetValues(typeof(UpdateBudgetCategory)))
            {
                PerformanceBudgetSnapshot snap = _gate.GetSnapshot(cat);
                if (!snap.OverBudget || snap.RunCount == 0)
                {
                    continue;
                }

                ModLogger.Warn(
                    $"{ModConstants.ModuleId}: perf over budget: {cat} took {snap.LastDurationSeconds:F3}s (interval {snap.ConfiguredIntervalSeconds:F3}s).");
                break;
            }
        }

        public string BuildScanRateSummary()
        {
            var sb = new StringBuilder(256);
            foreach (UpdateBudgetCategory cat in System.Enum.GetValues(typeof(UpdateBudgetCategory)))
            {
                PerformanceBudgetSnapshot s = _gate.GetSnapshot(cat);
                if (sb.Length > 0)
                {
                    sb.Append("; ");
                }

                sb.Append(cat)
                    .Append(": iv=")
                    .Append(s.ConfiguredIntervalSeconds.ToString("F2"))
                    .Append("s runs=")
                    .Append(s.RunCount)
                    .Append(" skip=")
                    .Append(s.SkippedCount);
            }

            return sb.ToString();
        }

        public void Reset()
        {
            _warnAccumSeconds = 0f;
        }
    }
}
