using TaleWorlds.MountAndBlade;

namespace Bannerlord.RTSCameraLite.Doctrine
{
    /// <summary>
    /// Per-agent rally/absorption tracking (Slice 12).
    /// </summary>
    public sealed class TroopAbsorptionRecord
    {
        public TroopAbsorptionRecord(
            Agent agent,
            TroopFormationState state,
            float distanceToCommander,
            FormationSlotAssignment assignedSlot,
            float lastAssignedTime,
            string reason)
        {
            Agent = agent;
            State = state;
            DistanceToCommander = distanceToCommander;
            AssignedSlot = assignedSlot;
            LastAssignedTime = lastAssignedTime;
            Reason = reason ?? string.Empty;
        }

        public Agent Agent { get; }

        public TroopFormationState State { get; set; }

        public float DistanceToCommander { get; set; }

        public FormationSlotAssignment AssignedSlot { get; set; }

        public float LastAssignedTime { get; set; }

        public string Reason { get; set; }
    }
}
