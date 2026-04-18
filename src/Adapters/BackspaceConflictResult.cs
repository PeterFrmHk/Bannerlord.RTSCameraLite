namespace Bannerlord.RTSCameraLite.Adapters
{
    /// <summary>
    /// Optional telemetry from <see cref="BackspaceConflictGuard"/> (Slice 3).
    /// </summary>
    public readonly struct BackspaceConflictResult
    {
        public BackspaceConflictResult(bool suppressRequested, string message)
        {
            SuppressRequested = suppressRequested;
            Message = message ?? string.Empty;
        }

        /// <summary>Whether native Backspace handling should be treated as suppressed (Slice 3: always false).</summary>
        public bool SuppressRequested { get; }

        public string Message { get; }
    }
}
