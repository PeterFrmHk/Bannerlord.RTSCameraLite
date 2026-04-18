using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace Bannerlord.RTSCameraLite.Tactical
{
    /// <summary>Resolved world position for a formation-level command target (Slice 19).</summary>
    public readonly struct FormationTargetResult
    {
        private FormationTargetResult(bool success, Vec3 position, Formation formation, string message)
        {
            Success = success;
            Position = position;
            TargetFormation = formation;
            Message = message ?? string.Empty;
        }

        public bool Success { get; }

        public Vec3 Position { get; }

        public Formation TargetFormation { get; }

        public string Message { get; }

        public static FormationTargetResult At(Vec3 worldPosition, Formation formation = null)
        {
            return new FormationTargetResult(true, worldPosition, formation, string.Empty);
        }

        public static FormationTargetResult Failure(string message)
        {
            return new FormationTargetResult(false, Vec3.Zero, null, message);
        }
    }
}
