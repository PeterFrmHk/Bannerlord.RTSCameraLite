namespace Bannerlord.RTSCameraLite.Adapters
{
    /// <summary>
    /// Describes whether native Backspace / order-menu suppression is available (Slice 7).
    /// </summary>
    public enum BackspaceConflictWireKind
    {
        /// <summary>Commander mode off or guard not evaluating.</summary>
        Inactive,

        /// <summary>Hook not connected to an engine input consumer (reserved).</summary>
        NotWired,

        /// <summary>Slice 0 research: no verified public managed API to block native order UI from seeing Backspace.</summary>
        Unsupported
    }

    /// <summary>
    /// Evaluation from <see cref="BackspaceConflictGuard"/> after <see cref="BackspaceConflictGuard.Tick"/> or lifecycle transitions.
    /// </summary>
    public readonly struct BackspaceConflictResult
    {
        private BackspaceConflictResult(
            bool suppressRequested,
            BackspaceConflictWireKind kind,
            string message)
        {
            SuppressRequested = suppressRequested;
            Kind = kind;
            Message = message ?? string.Empty;
        }

        /// <summary>
        /// When <c>true</c>, callers <i>would</i> attempt to skip forwarding Backspace to native systems.
        /// Slice 7: remains <c>false</c> because suppression is not wired — see <see cref="Kind"/>.
        /// </summary>
        public bool SuppressRequested { get; }

        public BackspaceConflictWireKind Kind { get; }

        public string Message { get; }

        public static BackspaceConflictResult Inactive(string message = "")
        {
            return new BackspaceConflictResult(false, BackspaceConflictWireKind.Inactive, string.IsNullOrEmpty(message) ? "inactive" : message);
        }

        public static BackspaceConflictResult NotWired(string message)
        {
            return new BackspaceConflictResult(false, BackspaceConflictWireKind.NotWired, message);
        }

        public static BackspaceConflictResult Unsupported(string message)
        {
            return new BackspaceConflictResult(false, BackspaceConflictWireKind.Unsupported, message);
        }
    }
}
