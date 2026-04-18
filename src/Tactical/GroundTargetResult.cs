using TaleWorlds.Library;

namespace Bannerlord.RTSCameraLite.Tactical
{
    /// <summary>
    /// Result of resolving a ground point for positional RTS commands (slice 11).
    /// </summary>
    public readonly struct GroundTargetResult
    {
        private GroundTargetResult(bool success, Vec3 position, string message)
        {
            Success = success;
            Position = position;
            Message = message ?? string.Empty;
        }

        public bool Success { get; }

        public Vec3 Position { get; }

        public string Message { get; }

        public static GroundTargetResult SuccessAt(Vec3 position)
        {
            return new GroundTargetResult(true, position, string.Empty);
        }

        public static GroundTargetResult Failure(string message)
        {
            return new GroundTargetResult(false, Vec3.Zero, message);
        }
    }
}
