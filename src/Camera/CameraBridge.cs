namespace Bannerlord.RTSCameraLite.Camera
{
    /// <summary>
    /// Single place for Bannerlord mission camera mutations (Slice 3: not wired yet).
    /// </summary>
    public static class CameraBridge
    {
        /// <summary>
        /// Attempts to apply <paramref name="pose"/> to the active mission camera.
        /// All version-sensitive camera API calls must stay in this type.
        /// </summary>
        public static CameraBridgeResult TryApply(TaleWorlds.MountAndBlade.Mission mission, RTSCameraPose pose)
        {
            if (mission == null)
            {
                return new CameraBridgeResult(false, "Camera bridge: null mission.");
            }

            try
            {
                // Slice 3: no confirmed cross-version camera wiring here yet.
                // Future: MissionScreen / custom camera stack hooks live only in this method.
                _ = pose;
                return CameraBridgeResult.NotWired();
            }
            catch (System.Exception ex)
            {
                return new CameraBridgeResult(false, "Camera bridge error: " + ex.Message);
            }
        }
    }
}
