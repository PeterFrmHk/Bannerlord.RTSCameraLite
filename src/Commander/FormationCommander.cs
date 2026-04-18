using TaleWorlds.MountAndBlade;

namespace Bannerlord.RTSCameraLite.Commander
{
    /// <summary>
    /// Resolved leadership agent for a formation (Slice 8 — presence only, no orders).
    /// </summary>
    public sealed class FormationCommander
    {
        public FormationCommander(
            Agent commanderAgent,
            bool isHero,
            bool isCaptain,
            bool isAlive,
            bool isMounted,
            float leadershipScore,
            float tacticsScore,
            float commandAuthorityScore,
            string source,
            string debugName)
        {
            CommanderAgent = commanderAgent;
            IsHero = isHero;
            IsCaptain = isCaptain;
            IsAlive = isAlive;
            IsMounted = isMounted;
            LeadershipScore = leadershipScore;
            TacticsScore = tacticsScore;
            CommandAuthorityScore = commandAuthorityScore;
            Source = source ?? string.Empty;
            DebugName = debugName ?? string.Empty;
        }

        public Agent CommanderAgent { get; }

        public bool IsHero { get; }

        public bool IsCaptain { get; }

        public bool IsAlive { get; }

        public bool IsMounted { get; }

        /// <summary>Normalized 0..1 (skill read or neutral estimate).</summary>
        public float LeadershipScore { get; }

        /// <summary>Normalized 0..1 (skill read or neutral estimate).</summary>
        public float TacticsScore { get; }

        /// <summary>Combined authority 0..1 after role bonuses and clamps.</summary>
        public float CommandAuthorityScore { get; }

        public string Source { get; }

        public string DebugName { get; }
    }
}
