namespace Bannerlord.RTSCameraLite.Equipment
{
    /// <summary>
    /// Aggregated equipment and role mix for one formation (Slice 10 — data only).
    /// </summary>
    public sealed class FormationCompositionProfile
    {
        public int AgentCount { get; set; }

        public float ShieldRatio { get; set; }

        public float PolearmRatio { get; set; }

        public float ShockInfantryRatio { get; set; }

        public float RangedRatio { get; set; }

        public float CavalryRatio { get; set; }

        public float HorseArcherRatio { get; set; }

        public float HeavyArmorEstimate { get; set; }

        public float AverageRankEstimate { get; set; }

        public EquipmentRole DominantRole { get; set; }

        public bool IsMountedHeavy { get; set; }

        public bool IsRangedHeavy { get; set; }

        public bool IsInfantryHeavy { get; set; }

        public string Reason { get; set; } = string.Empty;

        /// <summary>True when role reads were mostly confident (Slice 10 / Slice 11 eligibility).</summary>
        public bool IsCertain { get; set; }

        public static FormationCompositionProfile Empty(string reason = "")
        {
            return new FormationCompositionProfile
            {
                AgentCount = 0,
                DominantRole = EquipmentRole.Unknown,
                Reason = reason ?? string.Empty,
                IsCertain = false
            };
        }
    }
}
