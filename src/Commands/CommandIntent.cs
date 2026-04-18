using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace Bannerlord.RTSCameraLite.Commands
{
    /// <summary>Describes a command request before restriction and native execution (Slice 15).</summary>
    public sealed class CommandIntent
    {
        public CommandType Type { get; set; }

        public Formation SourceFormation { get; set; }

        public Formation TargetFormation { get; set; }

        public Vec3? TargetPosition { get; set; }

        public Vec2? TargetDirection { get; set; }

        public string Source { get; set; } = string.Empty;

        public bool RequiresPosition { get; set; }

        public bool RequiresTargetFormation { get; set; }

        public bool RequiresCommander { get; set; }
    }
}
