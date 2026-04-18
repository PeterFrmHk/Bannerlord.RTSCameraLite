using Bannerlord.RTSCameraLite.Camera;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.View.MissionViews;

namespace Bannerlord.RTSCameraLite.Tactical
{
    /// <summary>
    /// Resolves battlefield ground targets from camera (and optionally cursor) for RTS commands.
    /// </summary>
    internal sealed class GroundTargetResolver
    {
        private readonly TerrainProjectionService _terrain = new TerrainProjectionService();

        public GroundTargetResult TryResolveFromCamera(
            TaleWorlds.MountAndBlade.Mission mission,
            RTSCameraPose pose,
            float forwardDistance = TerrainProjectionService.DefaultForwardDistance)
        {
            if (pose == null)
            {
                return GroundTargetResult.Failure("No camera pose.");
            }

            return _terrain.TryProjectCameraForwardGround(
                mission,
                pose.Position,
                pose.Yaw,
                pose.Pitch,
                forwardDistance);
        }

        /// <summary>
        /// Cursor-to-world is not wired in slice 11; falls back to <see cref="TryResolveFromCamera"/>.
        /// </summary>
        public GroundTargetResult TryResolveFromCursor(
            MissionView missionView,
            TaleWorlds.MountAndBlade.Mission mission,
            RTSCameraPose pose,
            float forwardDistance = TerrainProjectionService.DefaultForwardDistance)
        {
            _ = missionView;
            return TryResolveFromCamera(mission, pose, forwardDistance);
        }
    }
}
