namespace Bannerlord.RTSCameraLite.Doctrine
{
    /// <summary>
    /// Rally and absorption lifecycle for a troop relative to the commander nucleus (Slice 12 — planning only).
    /// </summary>
    public enum TroopFormationState
    {
        Detached = 0,
        RallyingToCommander = 1,
        InsideAbsorptionRadius = 2,
        AssignedToFormationSlot = 3,
        SettlingIntoRank = 4,
        HoldingFormation = 5,
        Straggler = 6,
        BrokenMorale = 7,
        CommanderDead = 8,
        FormationDissolved = 9
    }
}
