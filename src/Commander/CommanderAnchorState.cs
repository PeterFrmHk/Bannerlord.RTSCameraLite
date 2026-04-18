using TaleWorlds.Library;

namespace Bannerlord.RTSCameraLite.Commander
{
    /// <summary>
    /// Preferred commander position behind a formation (Slice 9 — data only, no movement).
    /// </summary>
    public readonly struct CommanderAnchorState
    {
        public CommanderAnchorState(
            bool hasAnchor,
            Vec3 preferredPosition,
            Vec2 preferredFacing,
            float allowedRadius,
            bool commanderInsideAnchorZone,
            float commanderDistanceFromAnchor,
            string reason,
            bool isCertain)
        {
            HasAnchor = hasAnchor;
            PreferredPosition = preferredPosition;
            PreferredFacing = preferredFacing;
            AllowedRadius = allowedRadius;
            CommanderInsideAnchorZone = commanderInsideAnchorZone;
            CommanderDistanceFromAnchor = commanderDistanceFromAnchor;
            Reason = reason ?? string.Empty;
            IsCertain = isCertain;
        }

        public bool HasAnchor { get; }

        public Vec3 PreferredPosition { get; }

        /// <summary>Unit forward on the ground plane (formation facing).</summary>
        public Vec2 PreferredFacing { get; }

        public float AllowedRadius { get; }

        public bool CommanderInsideAnchorZone { get; }

        public float CommanderDistanceFromAnchor { get; }

        public string Reason { get; }

        /// <summary>False when facing, center, or commander presence was inferred or partial.</summary>
        public bool IsCertain { get; }

        public static CommanderAnchorState None(string reason, bool isCertain = true)
        {
            return new CommanderAnchorState(
                false,
                default,
                default,
                0f,
                false,
                0f,
                reason ?? string.Empty,
                isCertain);
        }
    }
}
