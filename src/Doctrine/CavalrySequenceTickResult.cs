using Bannerlord.RTSCameraLite.Commands;

namespace Bannerlord.RTSCameraLite.Doctrine
{
    /// <summary>Outcome of one throttled tick of <see cref="CavalryNativeChargeOrchestrator"/> (Slice 16).</summary>
    public sealed class CavalrySequenceTickResult
    {
        public CavalrySequenceTickResult(
            bool continued,
            bool completed,
            bool aborted,
            CavalryChargeState newState,
            string message,
            NativeOrderResult nativeOrderResult)
        {
            Continued = continued;
            Completed = completed;
            Aborted = aborted;
            NewState = newState;
            Message = message ?? string.Empty;
            NativeOrderResult = nativeOrderResult
                ?? NativeOrderResult.Failure(NativeOrderPrimitive.None, "CavalrySequenceTickResult: null native result");
        }

        public bool Continued { get; }

        public bool Completed { get; }

        public bool Aborted { get; }

        public CavalryChargeState NewState { get; }

        public string Message { get; }

        public NativeOrderResult NativeOrderResult { get; }
    }
}
