using System;
using Bannerlord.RTSCameraLite.Commander;
using Bannerlord.RTSCameraLite.Config;
using TaleWorlds.MountAndBlade;

namespace Bannerlord.RTSCameraLite.Doctrine
{
    /// <summary>
    /// When disciplined cavalry reform is allowed after lock release and disengagement (Slice 13).
    /// </summary>
    public static class CavalryReformPolicy
    {
        public static bool TryEvaluateReformDisciplineAllowed(
            bool positionLockWasReleased,
            CommanderPresenceResult presence,
            CommanderConfig config,
            float distanceToPrimaryTarget,
            float distanceFromImpactPosition,
            float nearestEnemyDistance,
            bool nearestEnemyDistanceKnown,
            float timeSinceLockReleaseSeconds,
            out string reason)
        {
            reason = string.Empty;
            CommanderConfig c = config ?? CommanderConfigDefaults.CreateDefault();

            try
            {
                bool commanderOk = IsCommanderValidForDisciplinedReform(presence, c);
                if (!commanderOk)
                {
                    reason = "commander invalid for disciplined reform";
                    return false;
                }

                if (!positionLockWasReleased)
                {
                    reason = "lock never released";
                    return false;
                }

                float disengage = distanceFromImpactPosition;
                if (float.IsNaN(disengage) || float.IsInfinity(disengage))
                {
                    disengage = distanceToPrimaryTarget;
                }

                if (disengage < c.CavalryReformDistanceFromAttackedFormation)
                {
                    reason = "inside reform distance";
                    return false;
                }

                if (timeSinceLockReleaseSeconds < c.CavalryReformCooldownSeconds)
                {
                    reason = "reform cooldown";
                    return false;
                }

                if (nearestEnemyDistanceKnown)
                {
                    if (nearestEnemyDistance < c.CavalryMinimumEnemyDistanceToReform)
                    {
                        reason = "enemy too close for reform";
                        return false;
                    }
                }
                else if (distanceToPrimaryTarget < c.CavalryReformDistanceFromAttackedFormation * 0.85f)
                {
                    reason = "fallback: still near primary target";
                    return false;
                }

                reason = "reform discipline ok";
                return true;
            }
            catch (Exception ex)
            {
                reason = "reform policy suppressed: " + ex.Message;
                return false;
            }
        }

        private static bool IsCommanderValidForDisciplinedReform(CommanderPresenceResult presence, CommanderConfig config)
        {
            if (config.AllowCavalryReformWithoutCommander)
            {
                return true;
            }

            try
            {
                if (presence == null || !presence.HasCommander || presence.Commander?.CommanderAgent == null)
                {
                    return false;
                }

                Agent a = presence.Commander.CommanderAgent;
                return a.IsActive() && a.Health > 0.01f;
            }
            catch
            {
                return false;
            }
        }
    }
}
