namespace Bannerlord.RTSCameraLite.Performance
{
    /// <summary>
    /// Point-in-time stats for one <see cref="UpdateBudgetCategory"/> (Slice 24 diagnostics).
    /// </summary>
    public sealed class PerformanceBudgetSnapshot
    {
        public PerformanceBudgetSnapshot(
            UpdateBudgetCategory category,
            float configuredIntervalSeconds,
            float lastRunTimeSeconds,
            int runCount,
            int skippedCount,
            float? lastDurationSeconds,
            bool overBudget)
        {
            Category = category;
            ConfiguredIntervalSeconds = configuredIntervalSeconds;
            LastRunTimeSeconds = lastRunTimeSeconds;
            RunCount = runCount;
            SkippedCount = skippedCount;
            LastDurationSeconds = lastDurationSeconds;
            OverBudget = overBudget;
        }

        public UpdateBudgetCategory Category { get; }

        public float ConfiguredIntervalSeconds { get; }

        public float LastRunTimeSeconds { get; }

        public int RunCount { get; }

        public int SkippedCount { get; }

        public float? LastDurationSeconds { get; }

        public bool OverBudget { get; }
    }
}
