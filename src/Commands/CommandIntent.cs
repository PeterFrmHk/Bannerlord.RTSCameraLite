using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace Bannerlord.RTSCameraLite.Commands
{
    /// <summary>
    /// Describes what the player asked to do; validated by <see cref="CommandRouter"/> and optionally executed natively (slice 12+).
    /// </summary>
    public sealed class CommandIntent
    {
        public CommandType Type { get; set; }

        public Formation TargetFormation { get; set; }

        public Vec3? TargetPosition { get; set; }

        public Vec2? TargetDirection { get; set; }

        public string Source { get; set; } = string.Empty;

        public bool RequiresPosition { get; set; }

        public bool RequiresDirection { get; set; }

        /// <summary>
        /// True when <see cref="CommandContext.ResolvedGroundPosition"/> supplied <see cref="TargetPosition"/> (slice 11).
        /// </summary>
        public bool FromResolvedGroundTarget { get; set; }
    }
}
