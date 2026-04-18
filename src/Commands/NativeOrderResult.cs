namespace Bannerlord.RTSCameraLite.Commands
{
    /// <summary>
    /// Outcome of <see cref="NativeOrderPrimitiveExecutor"/> (Slice 14 — never throws to callers).
    /// </summary>
    public sealed class NativeOrderResult
    {
        private NativeOrderResult(
            bool executed,
            bool notWired,
            bool blocked,
            NativeOrderPrimitive primitive,
            string message)
        {
            Executed = executed;
            NotWired = notWired;
            Blocked = blocked;
            Primitive = primitive;
            Message = message ?? string.Empty;
        }

        public bool Executed { get; }

        public bool NotWired { get; }

        public bool Blocked { get; }

        public NativeOrderPrimitive Primitive { get; }

        public string Message { get; }

        public static NativeOrderResult Success(NativeOrderPrimitive primitive, string message = "")
        {
            return new NativeOrderResult(true, false, false, primitive, message ?? string.Empty);
        }

        public static NativeOrderResult Failure(NativeOrderPrimitive primitive, string message)
        {
            return new NativeOrderResult(false, false, false, primitive, message ?? "failure");
        }

        public static NativeOrderResult NotWiredResult(NativeOrderPrimitive primitive, string message)
        {
            return new NativeOrderResult(false, true, false, primitive, message ?? "not wired");
        }

        public static NativeOrderResult BlockedResult(NativeOrderPrimitive primitive, string message)
        {
            return new NativeOrderResult(false, false, true, primitive, message ?? "blocked");
        }
    }
}
