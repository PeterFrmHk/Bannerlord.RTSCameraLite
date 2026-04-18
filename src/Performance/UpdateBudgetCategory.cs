namespace Bannerlord.RTSCameraLite.Performance
{
    /// <summary>
    /// Slice 24 — logical buckets for throttled mission work (scans vs per-frame camera).
    /// </summary>
    public enum UpdateBudgetCategory
    {
        Targeting = 0,

        CommanderScan,

        DoctrineScan,

        EligibilityScan,

        RallyAbsorptionScan,

        CavalrySequenceTick,

        FeedbackTick,

        MarkerTick,

        DiagnosticsTick,

        ConfigReloadCheck
    }
}
