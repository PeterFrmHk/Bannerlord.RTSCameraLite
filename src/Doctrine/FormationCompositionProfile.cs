namespace Bannerlord.RTSCameraLite.Doctrine
{
    /// <summary>
    /// Equipment and class heuristics for a formation (Slice 11).
    /// </summary>
    public sealed class FormationCompositionProfile
    {
        public FormationCompositionProfile(
            float shieldRatio,
            float polearmRatio,
            float mountedRatio,
            float cavalryRatio,
            float horseArcherRatio,
            bool isCertain,
            string reason)
        {
            ShieldRatio = shieldRatio;
            PolearmRatio = polearmRatio;
            MountedRatio = mountedRatio;
            CavalryRatio = cavalryRatio;
            HorseArcherRatio = horseArcherRatio;
            IsCertain = isCertain;
            Reason = reason ?? string.Empty;
        }

        /// <summary>Agents carrying a shield weapon class / total agents.</summary>
        public float ShieldRatio { get; }

        /// <summary>Agents with polearm-class primaries / total agents.</summary>
        public float PolearmRatio { get; }

        /// <summary>Agents flagged as mounted / total agents.</summary>
        public float MountedRatio { get; }

        /// <summary>Mounted melee cavalry proxy (see <see cref="DoctrineScoreCalculator"/>).</summary>
        public float CavalryRatio { get; }

        /// <summary>Mounted agents with bow/crossbow-class weapons / total agents.</summary>
        public float HorseArcherRatio { get; }

        public bool IsCertain { get; }

        public string Reason { get; }
    }
}
