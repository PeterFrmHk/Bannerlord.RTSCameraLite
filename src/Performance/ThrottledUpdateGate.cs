using System;

namespace Bannerlord.RTSCameraLite.Performance
{
    /// <summary>
    /// Slice 24 — cadence gate for expensive mission passes (no threading; monotonic mission clock in seconds).
    /// </summary>
    public sealed class ThrottledUpdateGate
    {
        private readonly CommanderPerformanceBudget _budget;
        private readonly GateState[] _states;

        private struct GateState
        {
            public bool HasRun;

            public float LastRunTimeSeconds;

            public int RunCount;

            public int SkippedCount;

            public float LastDurationSeconds;

            public bool OverBudgetLastRun;
        }

        public ThrottledUpdateGate(CommanderPerformanceBudget budget)
        {
            _budget = budget ?? CommanderPerformanceBudget.CreateDefaults();
            int n = Enum.GetValues(typeof(UpdateBudgetCategory)).Length;
            _states = new GateState[n];
        }

        public bool ShouldRun(UpdateBudgetCategory category, float currentTimeSeconds)
        {
            int idx = (int)category;
            if (idx < 0 || idx >= _states.Length)
            {
                return true;
            }

            ref GateState s = ref _states[idx];
            float interval = _budget.GetInterval(category);
            if (interval <= 0f)
            {
                interval = 0.01f;
            }

            if (!s.HasRun)
            {
                return true;
            }

            float elapsed = currentTimeSeconds - s.LastRunTimeSeconds;
            if (elapsed >= interval)
            {
                return true;
            }

            s.SkippedCount++;
            return false;
        }

        public void MarkRun(UpdateBudgetCategory category, float currentTimeSeconds, float durationSeconds = -1f)
        {
            int idx = (int)category;
            if (idx < 0 || idx >= _states.Length)
            {
                return;
            }

            ref GateState s = ref _states[idx];
            s.LastRunTimeSeconds = currentTimeSeconds;
            s.HasRun = true;
            s.RunCount++;
            if (durationSeconds >= 0f && !float.IsNaN(durationSeconds) && !float.IsInfinity(durationSeconds))
            {
                s.LastDurationSeconds = durationSeconds;
                float interval = _budget.GetInterval(category);
                s.OverBudgetLastRun = interval > 0f && durationSeconds > interval;
            }
        }

        public void Reset(UpdateBudgetCategory category)
        {
            int idx = (int)category;
            if (idx >= 0 && idx < _states.Length)
            {
                _states[idx] = default;
            }
        }

        public void ResetAll()
        {
            for (int i = 0; i < _states.Length; i++)
            {
                _states[i] = default;
            }
        }

        public PerformanceBudgetSnapshot GetSnapshot(UpdateBudgetCategory category)
        {
            int idx = (int)category;
            if (idx < 0 || idx >= _states.Length)
            {
                return new PerformanceBudgetSnapshot(
                    category,
                    _budget.GetInterval(category),
                    0f,
                    0,
                    0,
                    null,
                    false);
            }

            GateState s = _states[idx];
            float? dur = s.RunCount > 0 && s.LastDurationSeconds >= 0f ? (float?)s.LastDurationSeconds : null;
            return new PerformanceBudgetSnapshot(
                category,
                _budget.GetInterval(category),
                s.LastRunTimeSeconds,
                s.RunCount,
                s.SkippedCount,
                dur,
                s.OverBudgetLastRun);
        }
    }
}
