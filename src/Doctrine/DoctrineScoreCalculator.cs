using System;
using System.Collections.Generic;
using Bannerlord.RTSCameraLite.Adapters;
using Bannerlord.RTSCameraLite.Commander;
using Bannerlord.RTSCameraLite.Equipment;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace Bannerlord.RTSCameraLite.Doctrine
{
    /// <summary>
    /// Computes <see cref="FormationDoctrineProfile"/> from composition, presence, and adapter-safe reads (Slice 10 — data only).
    /// </summary>
    public sealed class DoctrineScoreCalculator
    {
        private readonly FormationDataAdapter _adapter;

        public DoctrineScoreCalculator(FormationDataAdapter adapter)
        {
            _adapter = adapter ?? throw new ArgumentNullException(nameof(adapter));
        }

        /// <summary>
        /// Computes doctrine for one formation. Never throws to callers.
        /// </summary>
        public DoctrineScoreResult Compute(
            TaleWorlds.MountAndBlade.Mission mission,
            Formation formation,
            CommanderPresenceResult commander,
            DoctrineScoreSettings settings)
        {
            DoctrineScoreSettings w = settings ?? DoctrineScoreSettings.FromConfig(null);

            if (formation == null)
            {
                return DoctrineScoreResult.Failure("formation is null");
            }

            try
            {
                FormationCompositionProfile composition = FormationCompositionAnalyzer.Analyze(_adapter, formation);
                FormationDataResult agentsResult = _adapter.TryGetFormationAgents(formation);
                if (!agentsResult.Success || agentsResult.Agents == null || agentsResult.Agents.Count == 0)
                {
                    FormationDoctrineProfile emptyDoctrine = BuildEmptyDoctrine(
                        composition,
                        string.IsNullOrEmpty(agentsResult.Message) ? "no agents" : agentsResult.Message);
                    return DoctrineScoreResult.Success(emptyDoctrine, "empty or unreadable formation", usedFallbacks: true);
                }

                bool usedFallbacks = false;
                if (!composition.IsCertain)
                {
                    usedFallbacks = true;
                }

                List<Agent> active = FilterActiveAgents(agentsResult.Agents);
                if (active.Count == 0)
                {
                    FormationDoctrineProfile emptyDoctrine = BuildEmptyDoctrine(composition, "no active agents");
                    return DoctrineScoreResult.Success(emptyDoctrine, "no active agents", usedFallbacks: true);
                }

                float moraleScore;
                float healthAvg;
                float shockFromHealth;
                SampleMoraleAndHealth(active, out moraleScore, out healthAvg, out shockFromHealth, ref usedFallbacks);

                float rankQuality = composition.AverageRankEstimate;
                if (rankQuality <= 0f)
                {
                    rankQuality = 0.35f;
                    usedFallbacks = true;
                }

                float commanderScore = EstimateCommanderScore(commander, ref usedFallbacks);
                float trainingScore = Clamp01(0.55f * rankQuality + 0.45f * (0.35f + 0.65f * commanderScore));
                float equipmentScore = EstimateEquipmentScore(composition);
                float cohesionScore = EstimateCohesion(mission, formation, active, ref usedFallbacks);
                float casualtyShock = Clamp01(shockFromHealth);

                bool presenceCertain = commander != null && commander.IsCertain;
                bool doctrineCertain = composition.IsCertain && presenceCertain;
                string reason = doctrineCertain ? "doctrine computed" : "doctrine partially estimated";

                float discipline = SynthesizeDiscipline(
                    moraleScore,
                    trainingScore,
                    equipmentScore,
                    commanderScore,
                    cohesionScore,
                    rankQuality,
                    casualtyShock,
                    w);

                FormationDoctrineProfile profile = new FormationDoctrineProfile(
                    moraleScore,
                    trainingScore,
                    equipmentScore,
                    commanderScore,
                    cohesionScore,
                    casualtyShock,
                    rankQuality,
                    discipline,
                    composition,
                    reason,
                    doctrineCertain);

                if (!doctrineCertain)
                {
                    return DoctrineScoreResult.Uncertain(profile, reason, usedFallbacks: usedFallbacks);
                }

                return DoctrineScoreResult.Success(profile, reason, usedFallbacks: usedFallbacks);
            }
            catch (Exception ex)
            {
                return DoctrineScoreResult.Failure("Compute threw: " + ex.Message);
            }
        }

        private static FormationDoctrineProfile BuildEmptyDoctrine(FormationCompositionProfile composition, string detail)
        {
            FormationCompositionProfile comp = composition ?? FormationCompositionProfile.Empty(detail);
            return new FormationDoctrineProfile(
                0f,
                0f,
                0f,
                0f,
                0f,
                0f,
                0f,
                0f,
                comp,
                detail,
                isCertain: false);
        }

        private static List<Agent> FilterActiveAgents(IReadOnlyList<Agent> agents)
        {
            var list = new List<Agent>(agents.Count);
            for (int i = 0; i < agents.Count; i++)
            {
                Agent a = agents[i];
                if (a != null && a.IsActive())
                {
                    list.Add(a);
                }
            }

            return list;
        }

        private void SampleMoraleAndHealth(
            List<Agent> active,
            out float moraleScore,
            out float healthAvg,
            out float shockFromHealth,
            ref bool usedFallbacks)
        {
            float moraleAcc = 0f;
            float healthAcc = 0f;
            int moraleSamples = 0;
            int n = active.Count;
            for (int i = 0; i < n; i++)
            {
                Agent agent = active[i];
                FormationDataResult moraleProbe = _adapter.TryGetAgentMorale01(agent);
                if (moraleProbe.Success)
                {
                    moraleAcc += moraleProbe.FloatValue;
                    moraleSamples++;
                }

                FormationDataResult healthProbe = _adapter.TryGetAgentHealthRatio(agent);
                if (healthProbe.Success)
                {
                    healthAcc += healthProbe.FloatValue;
                }
                else
                {
                    healthAcc += 0.55f;
                    usedFallbacks = true;
                }
            }

            healthAvg = healthAcc / Math.Max(1, n);
            shockFromHealth = Clamp01(1f - healthAvg);

            if (moraleSamples > 0)
            {
                moraleScore = Clamp01(moraleAcc / moraleSamples);
            }
            else
            {
                moraleScore = Clamp01(0.35f + healthAvg * 0.45f);
                usedFallbacks = true;
            }
        }

        private static float EstimateCommanderScore(CommanderPresenceResult commander, ref bool usedFallbacks)
        {
            if (commander == null || !commander.HasCommander || commander.Commander == null)
            {
                usedFallbacks = true;
                return 0.35f;
            }

            FormationCommander c = commander.Commander;
            float v = (c.LeadershipScore + c.TacticsScore + c.CommandAuthorityScore) / 3f;
            return Clamp01(v);
        }

        private static float EstimateEquipmentScore(FormationCompositionProfile composition)
        {
            if (composition == null)
            {
                return 0.45f;
            }

            float ranged = composition.RangedRatio;
            float meleeFoot = composition.ShieldRatio + composition.PolearmRatio + composition.ShockInfantryRatio;
            float mounted = Math.Max(composition.CavalryRatio, composition.HorseArcherRatio);
            float raw =
                0.35f * composition.HeavyArmorEstimate
                + 0.2f * composition.ShieldRatio
                + 0.12f * composition.PolearmRatio
                + 0.1f * composition.ShockInfantryRatio
                + 0.13f * ranged
                + 0.1f * mounted;
            return Clamp01(raw * 1.15f);
        }

        private float EstimateCohesion(
            TaleWorlds.MountAndBlade.Mission mission,
            Formation formation,
            List<Agent> active,
            ref bool usedFallbacks)
        {
            if (mission == null)
            {
                usedFallbacks = true;
                return 0.5f;
            }

            try
            {
                FormationDataResult centerRes = _adapter.TryGetFormationCenter(formation);
                if (!centerRes.Success)
                {
                    usedFallbacks = true;
                    return 0.5f;
                }

                Vec3 center = centerRes.Vec3;
                float acc = 0f;
                int counted = 0;
                for (int i = 0; i < active.Count; i++)
                {
                    FormationDataResult pos = _adapter.TryGetAgentPosition(active[i]);
                    if (!pos.Success)
                    {
                        continue;
                    }

                    counted++;
                    acc += (center - pos.Vec3).Length;
                }

                if (counted <= 0)
                {
                    usedFallbacks = true;
                    return 0.5f;
                }

                float avgSpread = acc / counted;
                float cohesion = 1f / (1f + avgSpread / 8f);
                return Clamp01(cohesion);
            }
            catch
            {
                usedFallbacks = true;
                return 0.5f;
            }
        }

        private static float SynthesizeDiscipline(
            float morale,
            float training,
            float equipment,
            float commander,
            float cohesion,
            float rank,
            float shock,
            DoctrineScoreSettings w)
        {
            float sum =
                morale * w.MoraleWeight
                + training * w.TrainingWeight
                + equipment * w.EquipmentWeight
                + commander * w.CommanderWeight
                + cohesion * w.CohesionWeight
                + rank * w.RankWeight;
            sum -= shock * w.CasualtyShockPenaltyWeight;
            return Clamp01(sum);
        }

        private static float Clamp01(float v)
        {
            if (float.IsNaN(v) || float.IsInfinity(v))
            {
                return 0f;
            }

            return MBMath.ClampFloat(v, 0f, 1f);
        }
    }
}
