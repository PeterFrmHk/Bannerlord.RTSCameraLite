using System;
using System.Collections.Generic;
using Bannerlord.RTSCameraLite.Adapters;
using Bannerlord.RTSCameraLite.Commander;
using Bannerlord.RTSCameraLite.Equipment;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace Bannerlord.RTSCameraLite.Doctrine
{
    /// <summary>
    /// Assigns planned slots only to troops inside absorption radius (Slice 12 — data only).
    /// </summary>
    public sealed class RowRankSlotAssigner
    {
        private readonly FormationDataAdapter _adapter;

        public RowRankSlotAssigner(FormationDataAdapter adapter)
        {
            _adapter = adapter ?? new FormationDataAdapter();
        }

        public void AssignSlots(
            TaleWorlds.MountAndBlade.Mission mission,
            Formation formation,
            CommanderPresenceResult presence,
            RowRankSpacingPlan plan,
            TroopAbsorptionController controller,
            CommanderRallySettings settings,
            float missionTime)
        {
            if (mission == null || formation == null || plan == null || controller == null || settings == null)
            {
                return;
            }

            FormationDataResult agentsResult = _adapter.TryGetFormationAgents(formation);
            if (!agentsResult.Success)
            {
                return;
            }

            Vec3 rally = ResolveRallyPoint(formation, presence);
            var absorbable = new List<Agent>();
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

                if (controller.TryGetRecord(formation, agent, out TroopAbsorptionRecord pre)
                    && (pre.State == TroopFormationState.BrokenMorale || pre.State == TroopFormationState.CommanderDead))
                {
                    continue;
                }

                CommanderAbsorptionZone.AbsorptionBand band = CommanderAbsorptionZone.Classify(rally, agent.Position, settings);
                if (band != CommanderAbsorptionZone.AbsorptionBand.InsideAbsorption)
                {
                    if (controller.TryGetRecord(formation, agent, out TroopAbsorptionRecord rec))
                    {
                        rec.AssignedSlot = null;
                        if (rec.State == TroopFormationState.AssignedToFormationSlot)
                        {
                            rec.State = band == CommanderAbsorptionZone.AbsorptionBand.Rallying
                                ? TroopFormationState.RallyingToCommander
                                : TroopFormationState.Straggler;
                        }
                    }

                    continue;
                }

                absorbable.Add(agent);
            }

            absorbable.Sort((a, b) => FrontScore(b, presence).CompareTo(FrontScore(a, presence)));

            int maxSlots = Math.Max(1, plan.RowCount * plan.RankDepth);
            int slotIndex = 0;
            for (int i = 0; i < absorbable.Count && slotIndex < maxSlots; i++)
            {
                Agent agent = absorbable[i];
                if (!controller.TryGetRecord(formation, agent, out TroopAbsorptionRecord record))
                {
                    continue;
                }

                if (record.AssignedSlot != null
                    && missionTime - record.LastAssignedTime < settings.SlotReassignmentCooldownSeconds)
                {
                    continue;
                }

                int row = PickRow(agent, presence, plan, slotIndex);
                int rank = slotIndex % Math.Max(1, plan.RankDepth);
                int file = slotIndex / Math.Max(1, plan.RankDepth);
                bool leftFlank = file == 0 && plan.RowCount > 2;
                bool rightFlank = file == plan.RowCount - 1 && plan.RowCount > 2;
                string tag = RowTag(agent, row);

                var assignment = new FormationSlotAssignment(row, rank, file, leftFlank, rightFlank, tag);
                record.AssignedSlot = assignment;
                record.LastAssignedTime = missionTime;
                record.State = TroopFormationState.AssignedToFormationSlot;
                record.Reason = "slot assigned";
                controller.SetRecord(formation, record);
                slotIndex++;
            }
        }

        private int PickRow(Agent agent, CommanderPresenceResult presence, RowRankSpacingPlan plan, int slotIndex)
        {
            float score = FrontScore(agent, presence);
            if (score >= 1.2f)
            {
                return 0;
            }

            if (score >= 0.75f)
            {
                return Math.Min(1, plan.RowCount - 1);
            }

            if (score < 0.35f)
            {
                return Math.Max(0, plan.RowCount - 1);
            }

            return Math.Min(plan.RowCount - 1, 1 + (slotIndex % Math.Max(1, plan.RowCount - 1)));
        }

        private static string RowTag(Agent agent, int row)
        {
            if (row == 0)
            {
                return "front";
            }

            if (row == 1)
            {
                return "second";
            }

            return "rear";
        }

        private float FrontScore(Agent agent, CommanderPresenceResult presence)
        {
            float score = 0f;
            try
            {
                EquipmentRole role = EquipmentRoleClassifier.Classify(_adapter, agent);
                if (role == EquipmentRole.Skirmisher || role == EquipmentRole.HorseArcher)
                {
                    score -= 0.35f;
                }

                if (role == EquipmentRole.ShockInfantry || role == EquipmentRole.Polearm)
                {
                    score += 0.15f;
                }
            }
            catch
            {
                // role uncertain — ignore
            }

            try
            {
                float morale = MBMath.ClampFloat(agent.Health / Math.Max(1f, agent.HealthLimit), 0f, 1f);
                score += morale * 0.35f;
            }
            catch
            {
                score += 0.2f;
            }

            try
            {
                if (agent.Character is BasicCharacterObject basic)
                {
                    score += MBMath.ClampFloat(basic.Level / 40f, 0f, 1f) * 0.25f;
                }
            }
            catch
            {
                score += 0.1f;
            }

            try
            {
                for (int si = (int)EquipmentIndex.Weapon0; si < (int)EquipmentIndex.NumPrimaryWeaponSlots; si++)
                {
                    MissionWeapon mw = agent.Equipment[(EquipmentIndex)si];
                    if (mw.IsEmpty)
                    {
                        continue;
                    }

                    WeaponClass wc = mw.CurrentUsageItem.WeaponClass;
                    if (wc == WeaponClass.SmallShield || wc == WeaponClass.LargeShield)
                    {
                        score += 0.45f;
                    }

                    if (wc == WeaponClass.OneHandedPolearm || wc == WeaponClass.TwoHandedPolearm || wc == WeaponClass.LowGripPolearm)
                    {
                        score += 0.25f;
                    }

                    if (wc == WeaponClass.Bow || wc == WeaponClass.Crossbow || wc == WeaponClass.Javelin)
                    {
                        score += 0.1f;
                    }
                }
            }
            catch
            {
                score += 0.05f;
            }

            return score;
        }

        private Vec3 ResolveRallyPoint(Formation formation, CommanderPresenceResult presence)
        {
            try
            {
                if (presence != null && presence.HasCommander && presence.Commander?.CommanderAgent != null)
                {
                    return presence.Commander.CommanderAgent.Position;
                }
            }
            catch
            {
                // ignored
            }

            FormationDataResult c = _adapter.TryGetFormationCenter(formation);
            return c.Success ? c.Vec3 : default;
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
