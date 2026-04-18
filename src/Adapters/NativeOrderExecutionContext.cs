namespace Bannerlord.RTSCameraLite.Adapters
{
    /// <summary>Optional tag for native primitive calls (Slice 16 — diagnostics / future routing).</summary>
    public sealed class NativeOrderExecutionContext
    {
        public NativeOrderExecutionContext(string tag)
        {
            Tag = tag ?? string.Empty;
        }

        public string Tag { get; }

        public static NativeOrderExecutionContext Default { get; } = new NativeOrderExecutionContext("default");

        public static NativeOrderExecutionContext CavalrySequenceProbe { get; } = new NativeOrderExecutionContext("cavalry_sequence_probe");

        public static NativeOrderExecutionContext CavalrySequenceTick { get; } = new NativeOrderExecutionContext("cavalry_sequence_tick");
    }
}
