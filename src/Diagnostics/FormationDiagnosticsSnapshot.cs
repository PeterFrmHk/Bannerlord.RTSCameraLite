namespace Bannerlord.RTSCameraLite.Diagnostics
{
    /// <summary>
    /// One-line-friendly capture of formation-internal state for portfolio / video diagnostics (Slice 20).
    /// </summary>
    public sealed class FormationDiagnosticsSnapshot
    {
        public FormationDiagnosticsSnapshot(
            string formationDebugName,
            string commanderPresenceSummary,
            string anchorSummary,
            float doctrineDisciplineScore,
            string dominantTroopRole,
            string eligibilitySummary,
            int rallyTotal,
            int rallyRallying,
            int rallyAbsorbable,
            int rallyAssigned,
            string cavalrySequenceSummary,
            string targetStateSummary,
            string nativeExecutorStatusSummary,
            long utcTicks)
        {
            FormationDebugName = formationDebugName ?? "?";
            CommanderPresenceSummary = commanderPresenceSummary ?? "?";
            AnchorSummary = anchorSummary ?? "?";
            DoctrineDisciplineScore = doctrineDisciplineScore;
            DominantTroopRole = dominantTroopRole ?? "?";
            EligibilitySummary = eligibilitySummary ?? "?";
            RallyTotal = rallyTotal;
            RallyRallying = rallyRallying;
            RallyAbsorbable = rallyAbsorbable;
            RallyAssigned = rallyAssigned;
            CavalrySequenceSummary = cavalrySequenceSummary ?? "—";
            TargetStateSummary = targetStateSummary ?? "—";
            NativeExecutorStatusSummary = nativeExecutorStatusSummary ?? "—";
            UtcTicks = utcTicks;
        }

        public string FormationDebugName { get; }

        public string CommanderPresenceSummary { get; }

        public string AnchorSummary { get; }

        public float DoctrineDisciplineScore { get; }

        public string DominantTroopRole { get; }

        public string EligibilitySummary { get; }

        public int RallyTotal { get; }

        public int RallyRallying { get; }

        public int RallyAbsorbable { get; }

        public int RallyAssigned { get; }

        public string CavalrySequenceSummary { get; }

        public string TargetStateSummary { get; }

        public string NativeExecutorStatusSummary { get; }

        public long UtcTicks { get; }
    }
}
