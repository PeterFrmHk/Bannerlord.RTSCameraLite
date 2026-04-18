using Bannerlord.RTSCameraLite.Commands;
using TaleWorlds.Library;

namespace Bannerlord.RTSCameraLite.UX
{
    /// <summary>
    /// Single active positional marker sample for minimal RTS feedback (slice 13).
    /// </summary>
    internal sealed class CommandMarkerState
    {
        public CommandMarkerState(Vec3 position, CommandType commandType, string label, float lifetimeSeconds)
        {
            Position = position;
            CommandType = commandType;
            Label = label ?? string.Empty;
            RemainingSeconds = lifetimeSeconds;
            Active = lifetimeSeconds > 0f;
        }

        public Vec3 Position { get; }

        public CommandType CommandType { get; }

        public float RemainingSeconds { get; internal set; }

        public string Label { get; }

        public bool Active { get; internal set; }
    }
}
