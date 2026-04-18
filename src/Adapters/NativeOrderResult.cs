namespace Bannerlord.RTSCameraLite.Adapters
{
    /// <summary>
    /// Result from <see cref="NativeOrderPrimitiveExecutor"/> (Slice 3 boundary).
    /// </summary>
    public sealed class NativeOrderResult
    {
        private NativeOrderResult(bool executed, string message, string primitiveName)
        {
            Executed = executed;
            Message = message ?? string.Empty;
            PrimitiveName = primitiveName ?? string.Empty;
        }

        public bool Executed { get; }

        public string Message { get; }

        public string PrimitiveName { get; }

        public static NativeOrderResult NotWired(string primitiveName, string reason)
        {
            return new NativeOrderResult(false, reason ?? "not wired", primitiveName ?? string.Empty);
        }

        public static NativeOrderResult Failure(string primitiveName, string message)
        {
            return new NativeOrderResult(false, message ?? "failure", primitiveName ?? string.Empty);
        }

        public static NativeOrderResult Success(string primitiveName, string message = "")
        {
            return new NativeOrderResult(true, message ?? string.Empty, primitiveName ?? string.Empty);
        }
    }
}
