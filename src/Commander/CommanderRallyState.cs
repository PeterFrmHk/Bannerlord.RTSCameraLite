using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace Bannerlord.RTSCameraLite.Commander
{
    /// <summary>
    /// Aggregated rally/absorption snapshot for one formation (Slice 12).
    /// </summary>
    public sealed class CommanderRallyState
    {
        public CommanderRallyState(
            Formation formation,
            FormationCommander commander,
            Vec3 rallyPoint,
            CommanderAnchorState anchorState,
            int totalTroops,
            int rallyingTroops,
            int absorbableTroops,
            int assignedTroops,
            int stragglers,
            string reason)
        {
            Formation = formation;
            Commander = commander;
            RallyPoint = rallyPoint;
            AnchorState = anchorState;
            TotalTroops = totalTroops;
            RallyingTroops = rallyingTroops;
            AbsorbableTroops = absorbableTroops;
            AssignedTroops = assignedTroops;
            Stragglers = stragglers;
            Reason = reason ?? string.Empty;
        }

        /// <summary>Formation this snapshot describes.</summary>
        public Formation Formation { get; }

        public FormationCommander Commander { get; }

        public Vec3 RallyPoint { get; }

        public CommanderAnchorState AnchorState { get; }

        public int TotalTroops { get; }

        public int RallyingTroops { get; }

        public int AbsorbableTroops { get; }

        public int AssignedTroops { get; }

        public int Stragglers { get; }

        public string Reason { get; }
    }
}
