using System;
using System.Collections.Generic;
using Bannerlord.RTSCameraLite.Adapters;
using Bannerlord.RTSCameraLite.Commander;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace Bannerlord.RTSCameraLite.Doctrine
{
    /// <summary>
    /// Tracks per-agent absorption state; only assigns slots inside absorption radius (Slice 12 — no movement).
    /// </summary>
    public sealed class TroopAbsorptionController
    {
        private readonly Dictionary<Formation, Dictionary<Agent, TroopAbsorptionRecord>> _records =
            new Dictionary<Formation, Dictionary<Agent, TroopAbsorptionRecord>>();

        /// <summary>
        /// Updates bands/states and clears dissolved formations. Slot assignment is delegated to <see cref="RowRankSlotAssigner"/>.
        /// </summary>
        public void SyncFormation(
            TaleWorlds.MountAndBlade.Mission mission,
            Formation formation,
            CommanderPresenceResult presence,
            Vec3 rallyPoint,
            CommanderRallySettings settings,
            float missionTime,
            RowRankSpacingPlan layoutPlan,
            RowRankSlotAssigner slotAssigner,
            FormationDataAdapter adapter)
        {
            if (formation == null || settings == null || adapter == null)
            {
                return;
            }

            try
            {
                if (formation.CountOfUnits <= 0)
                {
                    _records.Remove(formation);
                    return;
                }
            }
            catch
            {
                _records.Remove(formation);
                return;
            }

            if (!_records.TryGetValue(formation, out Dictionary<Agent, TroopAbsorptionRecord> map))
            {
                map = new Dictionary<Agent, TroopAbsorptionRecord>();
                _records[formation] = map;
            }

            bool commanderDead = IsCommanderDead(presence);
            FormationDataResult agentsResult = adapter.TryGetFormationAgents(formation);
            if (!agentsResult.Success)
            {
                return;
            }

            var seen = new HashSet<Agent>();
            for (int i = 0; i < agentsResult.Agents.Count; i++)
            {
                Agent agent = agentsResult.Agents[i];
                if (agent == null)
                {
                    continue;
                }

                seen.Add(agent);
                CommanderAbsorptionZone.AbsorptionBand band = CommanderAbsorptionZone.Classify(rallyPoint, agent.Position, settings);
                float dist = CommanderAbsorptionZone.PlanarDistance(rallyPoint, agent.Position);

                if (!map.TryGetValue(agent, out TroopAbsorptionRecord record))
                {
                    record = new TroopAbsorptionRecord(
                        agent,
                        TroopFormationState.Detached,
                        dist,
                        null,
                        0f,
                        string.Empty);
                    map[agent] = record;
                }

                record.DistanceToCommander = dist;

                try
                {
                    if (agent.HealthLimit > 1e-3f && agent.Health < agent.HealthLimit * 0.2f)
                    {
                        record.State = TroopFormationState.BrokenMorale;
                        record.AssignedSlot = null;
                        record.Reason = "broken morale";
                        continue;
                    }
                }
                catch
                {
                    // continue normal band mapping
                }

                if (commanderDead)
                {
                    record.State = TroopFormationState.CommanderDead;
                    record.AssignedSlot = null;
                    record.Reason = "commander dead";
                    continue;
                }

                TroopFormationState state = MapBandToState(band);
                record.State = state;
                record.Reason = band.ToString();
            }

            PruneMissingAgents(map, seen);

            if (!commanderDead && slotAssigner != null && layoutPlan != null)
            {
                slotAssigner.AssignSlots(
                    mission,
                    formation,
                    presence,
                    layoutPlan,
                    this,
                    settings,
                    missionTime);
            }
        }

        public int CountAssigned(Formation formation)
        {
            if (formation == null || !_records.TryGetValue(formation, out Dictionary<Agent, TroopAbsorptionRecord> map))
            {
                return 0;
            }

            int n = 0;
            foreach (KeyValuePair<Agent, TroopAbsorptionRecord> kv in map)
            {
                if (kv.Value?.AssignedSlot != null && kv.Value.State == TroopFormationState.AssignedToFormationSlot)
                {
                    n++;
                }
            }

            return n;
        }

        public int CountAbsorbable(
            Formation formation,
            CommanderPresenceResult presence,
            CommanderRallySettings settings,
            Vec3 rallyPoint,
            FormationDataAdapter adapter)
        {
            if (formation == null || settings == null || adapter == null)
            {
                return 0;
            }

            FormationDataResult agentsResult = adapter.TryGetFormationAgents(formation);
            if (!agentsResult.Success)
            {
                return 0;
            }

            int n = 0;
            for (int i = 0; i < agentsResult.Agents.Count; i++)
            {
                Agent agent = agentsResult.Agents[i];
                if (agent == null || !agent.IsActive())
                {
                    continue;
                }

                if (IsCommanderAgent(agent, presence))
                {
                    continue;
                }

                CommanderAbsorptionZone.AbsorptionBand band = CommanderAbsorptionZone.Classify(rallyPoint, agent.Position, settings);
                if (band == CommanderAbsorptionZone.AbsorptionBand.InsideAbsorption)
                {
                    n++;
                }
            }

            return n;
        }

        internal bool TryGetRecord(Formation formation, Agent agent, out TroopAbsorptionRecord record)
        {
            record = null;
            if (formation == null || agent == null)
            {
                return false;
            }

            return _records.TryGetValue(formation, out Dictionary<Agent, TroopAbsorptionRecord> map)
                   && map.TryGetValue(agent, out record);
        }

        internal void SetRecord(Formation formation, TroopAbsorptionRecord record)
        {
            if (formation == null || record?.Agent == null)
            {
                return;
            }

            if (!_records.TryGetValue(formation, out Dictionary<Agent, TroopAbsorptionRecord> map))
            {
                map = new Dictionary<Agent, TroopAbsorptionRecord>();
                _records[formation] = map;
            }

            map[record.Agent] = record;
        }

        public void Clear()
        {
            _records.Clear();
        }

        private static void PruneMissingAgents(Dictionary<Agent, TroopAbsorptionRecord> map, HashSet<Agent> seen)
        {
            var remove = new List<Agent>();
            foreach (KeyValuePair<Agent, TroopAbsorptionRecord> kv in map)
            {
                if (kv.Key == null || !seen.Contains(kv.Key))
                {
                    remove.Add(kv.Key);
                }
            }

            for (int i = 0; i < remove.Count; i++)
            {
                map.Remove(remove[i]);
            }
        }

        private static TroopFormationState MapBandToState(CommanderAbsorptionZone.AbsorptionBand band)
        {
            switch (band)
            {
                case CommanderAbsorptionZone.AbsorptionBand.InsideAbsorption:
                    return TroopFormationState.InsideAbsorptionRadius;
                case CommanderAbsorptionZone.AbsorptionBand.Rallying:
                    return TroopFormationState.RallyingToCommander;
                case CommanderAbsorptionZone.AbsorptionBand.OutsideRallyWithinCohesion:
                    return TroopFormationState.Detached;
                case CommanderAbsorptionZone.AbsorptionBand.OutsideCohesion:
                    return TroopFormationState.Straggler;
                default:
                    return TroopFormationState.Detached;
            }
        }

        private static bool IsCommanderDead(CommanderPresenceResult presence)
        {
            try
            {
                if (presence == null || !presence.HasCommander || presence.Commander?.CommanderAgent == null)
                {
                    return true;
                }

                Agent a = presence.Commander.CommanderAgent;
                return !a.IsActive() || a.Health <= 0.01f;
            }
            catch
            {
                return true;
            }
        }

        private static bool IsCommanderAgent(Agent agent, CommanderPresenceResult presence)
        {
            try
            {
                return presence != null
                       && presence.HasCommander
                       && presence.Commander?.CommanderAgent != null
                       && presence.Commander.CommanderAgent == agent;
            }
            catch
            {
                return false;
            }
        }
    }
}
