using Bannerlord.RTSCameraLite.Adapters;
using Bannerlord.RTSCameraLite.Doctrine;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace Bannerlord.RTSCameraLite.Commander
{
    /// <summary>
    /// Builds rally/absorption snapshots for a formation (Slice 12 — planning only).
    /// </summary>
    public sealed class CommanderRallyPlanner
    {
        private readonly FormationDataAdapter _adapter;

        public CommanderRallyPlanner(FormationDataAdapter adapter)
        {
            _adapter = adapter ?? new FormationDataAdapter();
        }

        /// <summary>Slice 12 API — uses config defaults and does not read absorption assignments.</summary>
        public CommanderRallyState BuildRallyState(
            TaleWorlds.MountAndBlade.Mission mission,
            Formation formation,
            CommanderPresenceResult commander,
            CommanderAnchorState anchor)
        {
            return BuildRallyState(
                mission,
                formation,
                commander,
                anchor,
                CommanderRallySettings.FromConfig(null),
                null);
        }

        public CommanderRallyState BuildRallyState(
            TaleWorlds.MountAndBlade.Mission mission,
            Formation formation,
            CommanderPresenceResult commander,
            CommanderAnchorState anchor,
            CommanderRallySettings rallySettings,
            TroopAbsorptionController absorptionController)
        {
            CommanderRallySettings s = rallySettings ?? CommanderRallySettings.FromConfig(null);
            CommanderAnchorState anchorSafe = anchor;

            if (formation == null)
            {
                return new CommanderRallyState(
                    null,
                    null,
                    default,
                    CommanderAnchorState.None("formation null"),
                    0,
                    0,
                    0,
                    0,
                    0,
                    "formation null");
            }

            try
            {
                if (formation.CountOfUnits <= 0)
                {
                    return new CommanderRallyState(
                        formation,
                        commander?.Commander,
                        default,
                        anchorSafe,
                        0,
                        0,
                        0,
                        0,
                        0,
                        "formation dissolved");
                }
            }
            catch
            {
                return new CommanderRallyState(
                    formation,
                    commander?.Commander,
                    default,
                    anchorSafe,
                    0,
                    0,
                    0,
                    0,
                    0,
                    "formation read threw");
            }

            Vec3 rallyPoint = ResolveRallyPoint(formation, commander, anchorSafe, _adapter);
            int total = 0;
            int rallying = 0;
            int absorbable = 0;
            int stragglers = 0;

            FormationDataResult agents = _adapter.TryGetFormationAgents(formation);
            if (agents.Success)
            {
                for (int i = 0; i < agents.Agents.Count; i++)
                {
                    Agent agent = agents.Agents[i];
                    if (agent == null || !agent.IsActive())
                    {
                        continue;
                    }

                    if (IsCommanderAgent(agent, commander))
                    {
                        continue;
                    }

                    total++;
                    CommanderAbsorptionZone.AbsorptionBand band = CommanderAbsorptionZone.Classify(rallyPoint, agent.Position, s);
                    switch (band)
                    {
                        case CommanderAbsorptionZone.AbsorptionBand.InsideAbsorption:
                            absorbable++;
                            break;
                        case CommanderAbsorptionZone.AbsorptionBand.Rallying:
                            rallying++;
                            break;
                        case CommanderAbsorptionZone.AbsorptionBand.OutsideCohesion:
                        case CommanderAbsorptionZone.AbsorptionBand.OutsideRallyWithinCohesion:
                            stragglers++;
                            break;
                    }
                }
            }

            int assigned = absorptionController != null ? absorptionController.CountAssigned(formation) : 0;

            string reason = commander != null && commander.HasCommander
                ? "rally nucleus active"
                : "degraded: no commander";

            return new CommanderRallyState(
                formation,
                commander?.Commander,
                rallyPoint,
                anchorSafe,
                total,
                rallying,
                absorbable,
                assigned,
                stragglers,
                reason);
        }

        private static Vec3 ResolveRallyPoint(
            Formation formation,
            CommanderPresenceResult commander,
            CommanderAnchorState anchor,
            FormationDataAdapter adapter)
        {
            try
            {
                if (commander != null && commander.HasCommander && commander.Commander?.CommanderAgent != null)
                {
                    Agent a = commander.Commander.CommanderAgent;
                    return a.Position;
                }
            }
            catch
            {
                // fall through
            }

            try
            {
                if (anchor.HasAnchor)
                {
                    return anchor.PreferredPosition;
                }
            }
            catch
            {
                // fall through
            }

            try
            {
                FormationDataAdapter ad = adapter ?? new FormationDataAdapter();
                FormationDataResult c = ad.TryGetFormationCenter(formation);
                if (c.Success)
                {
                    return c.Vec3;
                }
            }
            catch
            {
                // ignored
            }

            return default;
        }

        private static bool IsCommanderAgent(Agent agent, CommanderPresenceResult commander)
        {
            try
            {
                return commander != null
                       && commander.HasCommander
                       && commander.Commander?.CommanderAgent != null
                       && commander.Commander.CommanderAgent == agent;
            }
            catch
            {
                return false;
            }
        }
    }
}
