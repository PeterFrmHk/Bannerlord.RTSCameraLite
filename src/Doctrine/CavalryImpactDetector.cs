using System;
using Bannerlord.RTSCameraLite.Adapters;
using Bannerlord.RTSCameraLite.Config;
using Bannerlord.RTSCameraLite.Equipment;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace Bannerlord.RTSCameraLite.Doctrine
{
    /// <summary>
    /// Heuristic close contact / impact detection for cavalry doctrine (Slice 13 — best-effort, never throws out).
    /// </summary>
    public static class CavalryImpactDetector
    {
        public static bool TryDetectImpactOrCloseContact(
            Formation sourceFormation,
            Formation targetFormation,
            Vec3? targetPositionFallback,
            FormationCompositionProfile composition,
            CavalryChargeState approximateChargeState,
            float distanceToTarget,
            FormationDataAdapter adapter,
            CommanderConfig config,
            out bool closeContact,
            out bool impactContact,
            out string reason)
        {
            closeContact = false;
            impactContact = false;
            reason = string.Empty;

            try
            {
                CommanderConfig c = config ?? CommanderConfigDefaults.CreateDefault();

                if (!CavalrySpacingRules.IsCavalryHeavyFormation(composition))
                {
                    reason = "not cavalry-heavy";
                    return false;
                }

                if (approximateChargeState == CavalryChargeState.NotMountedFormation
                    || approximateChargeState == CavalryChargeState.CommanderDead)
                {
                    reason = "inactive charge state";
                    return false;
                }

                if (targetFormation == null && (!targetPositionFallback.HasValue))
                {
                    reason = "no target (distance-only unavailable)";
                    return false;
                }

                if (distanceToTarget <= c.CavalryImpactEnemyDistance)
                {
                    impactContact = true;
                    closeContact = true;
                    reason = "distance within impact radius";
                    return true;
                }

                if (distanceToTarget <= c.CavalryReleaseLockDistance)
                {
                    closeContact = true;
                    reason = "distance within release-lock radius";
                    return true;
                }

                if (TryDetectSpeedDrop(sourceFormation, adapter, c, out string speedReason))
                {
                    closeContact = true;
                    impactContact = distanceToTarget <= c.CavalryReleaseLockDistance * 1.35f;
                    reason = speedReason;
                    return true;
                }

                if (TryDetectEnemyAgentProximity(sourceFormation, targetFormation, adapter, c, out string proxReason))
                {
                    closeContact = true;
                    impactContact = true;
                    reason = proxReason;
                    return true;
                }

                reason = "distance-only: no contact";
                return false;
            }
            catch (Exception ex)
            {
                reason = "detector suppressed: " + ex.Message;
                return false;
            }
        }

        private static bool TryDetectSpeedDrop(
            Formation formation,
            FormationDataAdapter adapter,
            CommanderConfig config,
            out string reason)
        {
            reason = string.Empty;
            if (formation == null || adapter == null)
            {
                return false;
            }

            try
            {
                FormationDataResult agents = adapter.TryGetFormationAgents(formation);
                if (!agents.Success || agents.Agents == null || agents.Agents.Count == 0)
                {
                    return false;
                }

                int mounted = 0;
                float speedAcc = 0f;
                int speedSamples = 0;
                for (int i = 0; i < agents.Agents.Count; i++)
                {
                    Agent a = agents.Agents[i];
                    if (a == null || !a.IsActive())
                    {
                        continue;
                    }

                    if (a.MountAgent == null)
                    {
                        continue;
                    }

                    mounted++;
                    try
                    {
                        Vec2 v = a.MovementVelocity;
                        float mag = (float)Math.Sqrt(v.x * v.x + v.y * v.y);
                        speedAcc += mag;
                        speedSamples++;
                    }
                    catch
                    {
                        // ignore sample
                    }
                }

                if (speedSamples <= 0 || mounted <= 0)
                {
                    return false;
                }

                float mean = speedAcc / speedSamples;
                float ratio = MBMath.ClampFloat(mean / 8f, 0f, 1.5f);
                if (ratio < config.CavalryImpactSpeedDropThreshold)
                {
                    reason = "speed-drop proxy";
                    return true;
                }

                return false;
            }
            catch
            {
                return false;
            }
        }

        private static bool TryDetectEnemyAgentProximity(
            Formation source,
            Formation enemyFormation,
            FormationDataAdapter adapter,
            CommanderConfig config,
            out string reason)
        {
            reason = string.Empty;
            if (source == null || enemyFormation == null || adapter == null)
            {
                return false;
            }

            try
            {
                FormationDataResult src = adapter.TryGetFormationCenter(source);
                FormationDataResult en = adapter.TryGetFormationAgents(enemyFormation);
                if (!src.Success || !en.Success || en.Agents == null)
                {
                    return false;
                }

                int close = 0;
                int checkedN = 0;
                float threshold = Math.Max(2f, config.CavalryImpactEnemyDistance);
                for (int i = 0; i < en.Agents.Count && checkedN < 24; i++)
                {
                    Agent a = en.Agents[i];
                    if (a == null || !a.IsActive())
                    {
                        continue;
                    }

                    checkedN++;
                    float d = PlanarDistance(src.Vec3, a.Position);
                    if (d <= threshold)
                    {
                        close++;
                    }
                }

                if (checkedN <= 0)
                {
                    return false;
                }

                float ratio = (float)close / checkedN;
                if (ratio >= config.CavalryImpactAgentRatio)
                {
                    reason = "enemy-agent proximity ratio";
                    return true;
                }

                return false;
            }
            catch
            {
                return false;
            }
        }

        private static float PlanarDistance(Vec3 a, Vec3 b)
        {
            float dx = a.x - b.x;
            float dy = a.y - b.y;
            return (float)Math.Sqrt(dx * dx + dy * dy);
        }
    }
}
