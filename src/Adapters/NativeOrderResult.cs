namespace Bannerlord.RTSCameraLite.Adapters
{
    /// <summary>
    /// Result from <see cref="NativeOrderPrimitiveExecutor"/> (Slice 3 boundary).
    /// </summary>
    public sealed class NativeOrderResult
    {
        private NativeOrderResult(bool executed, string message, string primitiveName, bool notWired)
        {
            Executed = executed;
            Message = message ?? string.Empty;
            PrimitiveName = primitiveName ?? string.Empty;
            NotWired = notWired;
        }

        public bool Executed { get; }

        public string Message { get; }

        public string PrimitiveName { get; }

        /// <summary>True when the executor intentionally returns the Slice 3 skeleton (no engine order calls yet).</summary>
        public bool NotWired { get; }

        public static NativeOrderResult CreateNotWired(string primitiveName, string reason)
        {
            return new NativeOrderResult(false, reason ?? "not wired", primitiveName ?? string.Empty, notWired: true);
        }

        public static NativeOrderResult Failure(string primitiveName, string message)
        {
            return new NativeOrderResult(false, message ?? "failure", primitiveName ?? string.Empty, notWired: false);
        }

        public static NativeOrderResult Success(string primitiveName, string message = "")
        {
            return new NativeOrderResult(true, message ?? string.Empty, primitiveName ?? string.Empty, notWired: false);
        }
    }
}
