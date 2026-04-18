namespace Bannerlord.RTSCameraLite.Camera
{
    internal readonly struct CameraBridgeResult
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

        public static CameraBridgeResult Success()
        {
            return new CameraBridgeResult(true, "Camera bridge applied");
        }
    }
}
