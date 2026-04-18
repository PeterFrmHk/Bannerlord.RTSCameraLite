namespace Bannerlord.RTSCameraLite.UX
{
    /// <summary>Outcome of a single optional world render attempt for a command marker (Slice 19).</summary>
    public sealed class MarkerRenderResult
    {
        public MarkerRenderResult(bool visualRendered, string reason)
        {
            VisualRendered = visualRendered;
            Reason = reason ?? string.Empty;
        }

        public bool VisualRendered { get; }

        public string Reason { get; }

        public static MarkerRenderResult Skipped(string reason)
        {
            return new MarkerRenderResult(false, reason);
        }

        public static MarkerRenderResult VisualOk(string reason = "")
        {
            return new MarkerRenderResult(true, reason);
        }
    }
}
