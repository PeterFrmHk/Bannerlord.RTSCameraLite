using TaleWorlds.Library;

namespace Bannerlord.RTSCameraLite.UX
{
    /// <summary>One active temporary marker sample (Slice 19).</summary>
    public sealed class CommandMarkerState
    {
        public CommandMarkerState(
            bool active,
            CommandMarkerType type,
            Vec3 position,
            string label,
            float remainingSeconds,
            string source,
            bool visualRendered,
            string reason)
        {
            Active = active;
            Type = type;
            Position = position;
            Label = label ?? string.Empty;
            RemainingSeconds = remainingSeconds;
            Source = source ?? string.Empty;
            VisualRendered = visualRendered;
            Reason = reason ?? string.Empty;
        }

        public bool Active { get; set; }

        public CommandMarkerType Type { get; }

        public Vec3 Position { get; }

        public string Label { get; }

        public float RemainingSeconds { get; set; }

        public string Source { get; }

        public bool VisualRendered { get; }

        public string Reason { get; }
    }
}
