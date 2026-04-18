using Bannerlord.RTSCameraLite.Tactical;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace Bannerlord.RTSCameraLite.Commands
{
    /// <summary>
    /// World state used to validate a <see cref="CommandIntent"/> (no order execution).
    /// </summary>
    public sealed class CommandContext
    {
        public CommandContext(
            TaleWorlds.MountAndBlade.Mission mission,
            Formation selectedFormation,
            bool rtsModeEnabled,
            Vec3 cursorOrCameraFallbackWorldPosition,
            GroundTargetResult currentGroundTarget)
        {
            Mission = mission;
            SelectedFormation = selectedFormation;
            RtsModeEnabled = rtsModeEnabled;
            CursorOrCameraFallbackWorldPosition = cursorOrCameraFallbackWorldPosition;
            CurrentGroundTarget = currentGroundTarget;
        }

        public TaleWorlds.MountAndBlade.Mission Mission { get; }

        public Formation SelectedFormation { get; }

        public bool RtsModeEnabled { get; }

        /// <summary>
        /// Legacy fallback when no resolved ground target is available.
        /// </summary>
        public Vec3 CursorOrCameraFallbackWorldPosition { get; }

        /// <summary>
        /// Latest sampled ground target from <see cref="Bannerlord.RTSCameraLite.Tactical.GroundTargetResolver"/> while RTS is active.
        /// </summary>
        public GroundTargetResult CurrentGroundTarget { get; }

        /// <summary>
        /// Convenience: valid world position when <see cref="GroundTargetResult.Success"/> is true.
        /// </summary>
        public Vec3? ResolvedGroundPosition =>
            CurrentGroundTarget.Success ? CurrentGroundTarget.Position : (Vec3?)null;
    }
}
