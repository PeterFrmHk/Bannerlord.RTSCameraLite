namespace Bannerlord.RTSCameraLite.Camera
{
    /// <summary>
    /// Outcome of <see cref="CameraBridge.TryApply"/>; keep surface stable across game versions.
    /// </summary>
    public readonly struct CameraBridgeResult
    {
        public bool Applied { get; }
        public string Message { get; }

        public CameraBridgeResult(bool applied, string message)
        {
            Applied = applied;
            Message = message ?? string.Empty;
        }

        public static CameraBridgeResult NotWired()
        {
            return new CameraBridgeResult(false, "Camera bridge not wired");
        }
    }
}
