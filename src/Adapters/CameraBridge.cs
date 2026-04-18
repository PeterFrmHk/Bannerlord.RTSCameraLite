using Bannerlord.RTSCameraLite.Camera;

namespace Bannerlord.RTSCameraLite.Adapters
{
    /// <summary>
    /// Single choke point for version-sensitive camera APIs. Slice 3: not wired — all paths return
    /// <see cref="CameraBridgeResult.NotWired"/> until Slice 0 + ILSpy verification for the pinned game build.
    /// </summary>
    public sealed class CameraBridge
    {
        private const string NotWiredReason =
            "Slice 3 skeleton: no engine camera writes until MissionView + MissionScreen paths are verified (docs/research/base-game-camera-scan.md).";

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
