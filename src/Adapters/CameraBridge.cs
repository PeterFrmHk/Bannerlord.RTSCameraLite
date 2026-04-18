using Bannerlord.RTSCameraLite.Camera;

namespace Bannerlord.RTSCameraLite.Adapters
{
    /// <summary>
    /// Single choke point for version-sensitive camera APIs. Slice 4: still not wired — all paths return
    /// <see cref="CameraBridgeResult.NotWired"/> until engine apply/restore is verified for the pinned build.
    /// Internal pose updates still call <see cref="TryApply"/> each commander tick.
    /// </summary>
    public sealed class CameraBridge
    {
        private const string NotWiredReason =
            "Slice 4: no engine camera writes until MissionView + MissionScreen paths are verified (docs/research/base-game-camera-scan.md).";

        public CameraBridgeResult TryApply(TaleWorlds.MountAndBlade.Mission mission, CommanderCameraPose pose)
        {
            if (mission == null)
            {
                return CameraBridgeResult.Failure("mission is null");
            }

            _ = pose;
            return CameraBridgeResult.NotWired(NotWiredReason);
        }

        public CameraBridgeResult RestoreNativeCamera(TaleWorlds.MountAndBlade.Mission mission)
        {
            if (mission == null)
            {
                return CameraBridgeResult.Failure("mission is null");
            }

            return CameraBridgeResult.NotWired(NotWiredReason);
        }
    }
}
