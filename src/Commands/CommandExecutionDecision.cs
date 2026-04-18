using Bannerlord.RTSCameraLite.Adapters;

namespace Bannerlord.RTSCameraLite.Commands
{
    /// <summary>Whether and how a validated intent may touch native primitives (Slice 15).</summary>
    public sealed class CommandExecutionDecision
    {
        public CommandExecutionDecision(
            bool shouldExecute,
            bool requiresNativeOrder,
            bool requiresCavalrySequence,
            NativeOrderPrimitive nativePrimitive,
            string reason)
        {
            ShouldExecute = shouldExecute;
            RequiresNativeOrder = requiresNativeOrder;
            RequiresCavalrySequence = requiresCavalrySequence;
            NativePrimitive = nativePrimitive;
            Reason = reason ?? string.Empty;
        }

        public bool ShouldExecute { get; }

        public bool RequiresNativeOrder { get; }

        public bool RequiresCavalrySequence { get; }

        public NativeOrderPrimitive NativePrimitive { get; }

        public string Reason { get; }
    }
}
