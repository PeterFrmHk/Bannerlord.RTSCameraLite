namespace Bannerlord.RTSCameraLite.Diagnostics
{
    /// <summary>
    /// Accumulates mission time between diagnostics refresh emissions (Slice 20).
    /// </summary>
    public sealed class DiagnosticsThrottle
    {
        private float _accumSeconds;

        public void Reset()
        {
            _accumSeconds = 0f;
        }

        /// <summary>Forces the next <see cref="TryConsumeRefresh"/> to succeed (e.g. after toggling diagnostics on).</summary>
        public void BumpPastInterval()
        {
            _accumSeconds = 1e6f;
        }

        /// <summary>Returns true when the refresh interval has elapsed.</summary>
        public bool TryConsumeRefresh(float dt, float intervalSeconds)
        {
            float interval = intervalSeconds <= 0f ? 0.05f : intervalSeconds;
            _accumSeconds += dt;
            if (_accumSeconds >= interval)
            {
                _accumSeconds = 0f;
                return true;
            }

            return false;
        }
    }
}
