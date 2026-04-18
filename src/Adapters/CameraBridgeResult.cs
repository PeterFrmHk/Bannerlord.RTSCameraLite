namespace Bannerlord.RTSCameraLite.Adapters
{
    /// <summary>
    /// Outcome from <see cref="CameraBridge"/> apply/restore calls (Slice 3 boundary).
    /// </summary>
    public readonly struct CameraBridgeResult
    {
        public bool Applied { get; }

        public bool Restored { get; }

        public string Message { get; }

        private CameraBridgeResult(bool applied, bool restored, string message)
        {
            Applied = applied;
            Restored = restored;
            Message = message ?? string.Empty;
        }

        public static CameraBridgeResult Success(bool applied, bool restored, string message = "")
        {
            return new CameraBridgeResult(applied, restored, message);
        }

        public static CameraBridgeResult NotWired(string reason)
        {
            return new CameraBridgeResult(false, false, reason ?? "not wired");
        }

        public static CameraBridgeResult Failure(string message)
        {
            return new CameraBridgeResult(false, false, message ?? "failure");
        }
    }
}
