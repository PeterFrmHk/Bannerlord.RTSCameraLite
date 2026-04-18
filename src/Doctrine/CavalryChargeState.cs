namespace Bannerlord.RTSCameraLite.Doctrine
{
    /// <summary>
    /// High-level cavalry charge / lock / reform lifecycle (Slice 13 — planning only, no native orders).
    /// </summary>
    public enum CavalryChargeState
    {
        NotMountedFormation = 0,
        RallyingToCommander = 1,
        MountedFormationAssembling = 2,
        ChargeReady = 3,
        Charging = 4,
        CloseContact = 5,
        ImpactContact = 6,
        PositionLockReleased = 7,
        Disengaging = 8,
        ReformDistanceReached = 9,
        Reforming = 10,
        Reassembled = 11,
        ChargeBroken = 12,
        CommanderDead = 13,
        MoraleCollapse = 14
    }
}
