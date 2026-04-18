using System;
using Bannerlord.RTSCameraLite.Adapters;
using TaleWorlds.MountAndBlade;

namespace Bannerlord.RTSCameraLite.Equipment
{
    /// <summary>
    /// Builds <see cref="FormationCompositionProfile"/> from formation agents via <see cref="FormationDataAdapter"/> only (Slice 10).
    /// </summary>
    public static class FormationCompositionAnalyzer
    {
        public static FormationCompositionProfile Analyze(FormationDataAdapter adapter, Formation formation)
        {
            var profile = new FormationCompositionProfile
            {
                DominantRole = EquipmentRole.Unknown,
                Reason = string.Empty,
                IsCertain = false
            };

            if (adapter == null || formation == null)
            {
                profile.Reason = "null adapter or formation";
                return profile;
            }

            try
            {
                FormationDataResult agentsResult = adapter.TryGetFormationAgents(formation);
                if (!agentsResult.Success || agentsResult.Agents == null || agentsResult.Agents.Count == 0)
                {
                    profile.Reason = string.IsNullOrEmpty(agentsResult.Message) ? "no agents" : agentsResult.Message;
                    return profile;
                }

                int n = 0;
                int shield = 0;
                int polearm = 0;
                int shock = 0;
                int archer = 0;
                int cross = 0;
                int skirm = 0;
                int cav = 0;
                int ha = 0;
                int unknown = 0;
                float armorAcc = 0f;
                float rankAcc = 0f;
                int rankUncertain = 0;

                for (int i = 0; i < agentsResult.Agents.Count; i++)
                {
                    Agent agent = agentsResult.Agents[i];
                    if (agent == null || !agent.IsActive())
                    {
                        continue;
                    }

                    n++;
                    EquipmentRole role = EquipmentRoleClassifier.Classify(adapter, agent);
                    switch (role)
                    {
                        case EquipmentRole.ShieldInfantry:
                            shield++;
                            break;
                        case EquipmentRole.Polearm:
                            polearm++;
                            break;
                        case EquipmentRole.ShockInfantry:
                            shock++;
                            break;
                        case EquipmentRole.Archer:
                            archer++;
                            break;
                        case EquipmentRole.Crossbow:
                            cross++;
                            break;
                        case EquipmentRole.Skirmisher:
                            skirm++;
                            break;
                        case EquipmentRole.Cavalry:
                            cav++;
                            break;
                        case EquipmentRole.HorseArcher:
                            ha++;
                            break;
                        default:
                            unknown++;
                            break;
                    }

                    try
                    {
                        FormationDataResult hints = adapter.TryGetAgentEquipmentHints(agent);
                        armorAcc += hints.Success ? hints.FloatValue : 0.45f;
                    }
                    catch
                    {
                        armorAcc += 0.45f;
                    }

                    TroopRankEstimate rank = TroopRankClassifier.EstimateForAgent(adapter, agent);
                    rankAcc += rank.Rank01;
                    if (rank.IsUncertain)
                    {
                        rankUncertain++;
                    }
                }

                if (n <= 0)
                {
                    profile.Reason = "no active agents";
                    return profile;
                }

                float inv = 1f / n;
                profile.AgentCount = n;
                profile.ShieldRatio = shield * inv;
                profile.PolearmRatio = polearm * inv;
                profile.ShockInfantryRatio = shock * inv;
                int rangedCount = archer + cross + skirm;
                profile.RangedRatio = rangedCount * inv;
                profile.CavalryRatio = cav * inv;
                profile.HorseArcherRatio = ha * inv;
                profile.HeavyArmorEstimate = (float)Math.Max(0d, Math.Min(1d, armorAcc * inv));
                profile.AverageRankEstimate = (float)Math.Max(0d, Math.Min(1d, rankAcc * inv));
                profile.DominantRole = PickDominant(
                    shield,
                    polearm,
                    shock,
                    archer,
                    cross,
                    skirm,
                    cav,
                    ha,
                    unknown);
                float mountedShare = (cav + ha) * inv;
                profile.IsMountedHeavy = mountedShare > 0.5f;
                profile.IsRangedHeavy = profile.RangedRatio > 0.5f;
                float infShare = (shield + polearm + shock) * inv;
                profile.IsInfantryHeavy = infShare > 0.5f;
                float unknownShare = unknown * inv;
                profile.IsCertain = unknownShare < 0.35f && rankUncertain * inv < 0.5f;
                profile.Reason = profile.IsCertain ? "composition ok" : "partial unknown equipment or rank";
                return profile;
            }
            catch (Exception ex)
            {
                profile.Reason = "analyzer threw: " + ex.Message;
                profile.IsCertain = false;
                return profile;
            }
        }

        private static EquipmentRole PickDominant(
            int shield,
            int polearm,
            int shock,
            int archer,
            int cross,
            int skirm,
            int cav,
            int ha,
            int unknown)
        {
            int best = -1;
            EquipmentRole bestRole = EquipmentRole.Unknown;
            void consider(int count, EquipmentRole role)
            {
                if (count > best)
                {
                    best = count;
                    bestRole = role;
                }
            }

            consider(shield, EquipmentRole.ShieldInfantry);
            consider(polearm, EquipmentRole.Polearm);
            consider(shock, EquipmentRole.ShockInfantry);
            consider(archer, EquipmentRole.Archer);
            consider(cross, EquipmentRole.Crossbow);
            consider(skirm, EquipmentRole.Skirmisher);
            consider(cav, EquipmentRole.Cavalry);
            consider(ha, EquipmentRole.HorseArcher);
            consider(unknown, EquipmentRole.Unknown);
            return bestRole;
        }
    }
}
