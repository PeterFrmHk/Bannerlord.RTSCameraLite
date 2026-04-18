using System;
using Bannerlord.RTSCameraLite.Adapters;
using Bannerlord.RTSCameraLite.Commander;
using Bannerlord.RTSCameraLite.Config;
using Bannerlord.RTSCameraLite.Equipment;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace Bannerlord.RTSCameraLite.Doctrine
{
    /// <summary>
    /// Coordinates cavalry spacing context, impact heuristics, lock policy, and sequence state (Slice 13).
    /// </summary>
    public static class CavalryDoctrineRules
    {
        public static CavalryChargeSequenceState Evaluate(
            TaleWorlds.MountAndBlade.Mission mission,
            Formation sourceFormation,
            FormationCompositionProfile composition,
            FormationDoctrineProfile doctrine,
            CommanderPresenceResult presence,
            CommanderRallyState rally,
            CommanderConfig config,
            FormationDataAdapter adapter,
            RowRankSpacingPlan plan,
            CavalryChargeSequenceState previous,
            float deltaTime)
        {
            CommanderConfig c = config ?? CommanderConfigDefaults.CreateDefault();

            if (mission == null || sourceFormation == null || composition == null || doctrine == null)
            {
                return CavalryChargeSequenceState.NotMounted(sourceFormation, "null inputs");
            }

            if (!CavalrySpacingRules.IsCavalryHeavyFormation(composition))
            {
                return CavalryChargeSequenceState.NotMounted(sourceFormation, "not cavalry-heavy");
            }

            try
            {
                Formation target = FindNearestEnemyFormation(mission, sourceFormation, adapter);
                Vec3? targetPos = TryTargetPosition(target, adapter);
                float distTarget = ComputeDistanceToTarget(sourceFormation, target, targetPos, adapter);
                float nearestEnemy = ComputeNearestEnemyFormationDistance(mission, sourceFormation, adapter, out bool nearestKnown);

                bool commanderValid = IsCommanderAgentValid(presence);
                bool horseArcherHeavy = plan != null && plan.IsHorseArcherLayout
                                        || CavalrySpacingRules.IsHorseArcherHeavyFormation(composition);
                bool shockHeavy = CavalrySpacingRules.IsShockCavalryHeavyFormation(composition, sourceFormation);

                CavalryChargeState prevState = previous?.CurrentState ?? CavalryChargeState.MountedFormationAssembling;
                CavalryImpactDetector.TryDetectImpactOrCloseContact(
                    sourceFormation,
                    target,
                    targetPos,
                    composition,
                    prevState,
                    distTarget,
                    adapter,
                    c,
                    out bool closeContact,
                    out bool impactContact,
                    out string impactReason);

                bool releaseRecommended = CavalryPositionLockPolicy.ShouldReleasePositionLock(
                    distTarget,
                    closeContact || impactContact,
                    shockHeavy && !horseArcherHeavy,
                    c.CavalryReleaseLockDistance);

                bool lockReleased = previous?.PositionLockReleased == true || releaseRecommended;

                float timeSinceLock = 0f;
                if (lockReleased)
                {
                    timeSinceLock = (previous?.PositionLockReleased == true ? previous.TimeSinceLockRelease : 0f) + deltaTime;
                }

                Vec3? impactPos = previous?.ImpactPosition;
                if ((impactContact || closeContact) && targetPos.HasValue)
                {
                    impactPos = targetPos;
                }

                float distFromImpact = ComputeDistanceFromImpact(sourceFormation, impactPos, adapter, distTarget);

                bool reformDiscipline = CavalryReformPolicy.TryEvaluateReformDisciplineAllowed(
                    lockReleased,
                    presence,
                    c,
                    distTarget,
                    distFromImpact,
                    nearestEnemy,
                    nearestKnown,
                    timeSinceLock,
                    out string reformReason);

                bool reactivateLock = CavalryPositionLockPolicy.ShouldReactivatePositionLock(
                    lockReleased,
                    distTarget,
                    distFromImpact,
                    timeSinceLock,
                    c.CavalryReformDistanceFromAttackedFormation,
                    c.CavalryReformCooldownSeconds,
                    reformDiscipline,
                    c.CavalryReleaseLockDistance);

                CavalryChargeState state = ClassifyState(
                    doctrine,
                    rally,
                    commanderValid,
                    horseArcherHeavy,
                    shockHeavy,
                    distTarget,
                    lockReleased,
                    closeContact,
                    impactContact,
                    reformDiscipline,
                    reactivateLock,
                    distFromImpact,
                    timeSinceLock,
                    prevState,
                    c,
                    out string stateReason);

                string reason = $"{stateReason}; impact={impactReason}; reform={reformReason}";
                return new CavalryChargeSequenceState(
                    sourceFormation,
                    target,
                    state,
                    targetPos,
                    impactPos,
                    lockReleased,
                    reformDiscipline,
                    commanderValid,
                    distTarget,
                    distFromImpact,
                    timeSinceLock,
                    reason);
            }
            catch (Exception ex)
            {
                return new CavalryChargeSequenceState(
                    sourceFormation,
                    null,
                    CavalryChargeState.ChargeBroken,
                    null,
                    null,
                    false,
                    false,
                    false,
                    float.MaxValue,
                    float.MaxValue,
                    0f,
                    "evaluate threw: " + ex.Message);
            }
        }

        private static CavalryChargeState ClassifyState(
            FormationDoctrineProfile doctrine,
            CommanderRallyState rally,
            bool commanderValid,
            bool horseArcherHeavy,
            bool shockHeavy,
            float distTarget,
            bool lockReleased,
            bool closeContact,
            bool impactContact,
            bool reformDiscipline,
            bool reactivateLock,
            float distFromImpact,
            float timeSinceLock,
            CavalryChargeState prev,
            CommanderConfig cfg,
            out string stateReason)
        {
            stateReason = string.Empty;
            if (doctrine.MoraleScore < 0.12f)
            {
                stateReason = "morale collapsed";
                return CavalryChargeState.MoraleCollapse;
            }

            if (!commanderValid)
            {
                stateReason = "commander invalid";
                return CavalryChargeState.CommanderDead;
            }

            if (doctrine.MoraleScore < 0.2f && doctrine.CasualtyShock > 0.72f)
            {
                stateReason = "morale shock";
                return CavalryChargeState.ChargeBroken;
            }

            if (impactContact)
            {
                stateReason = "impact heuristic";
                return CavalryChargeState.ImpactContact;
            }

            if (closeContact)
            {
                stateReason = "close contact";
                return CavalryChargeState.CloseContact;
            }

            if (lockReleased && distTarget <= cfg.CavalryReleaseLockDistance)
            {
                stateReason = "lock released near target";
                return CavalryChargeState.PositionLockReleased;
            }

            if (lockReleased && reformDiscipline && reactivateLock)
            {
                stateReason = "re-lock gate passed";
                return CavalryChargeState.Reassembled;
            }

            if (lockReleased && reformDiscipline && distFromImpact >= cfg.CavalryReformDistanceFromAttackedFormation
                && timeSinceLock >= cfg.CavalryReformCooldownSeconds)
            {
                stateReason = "reforming discipline";
                return CavalryChargeState.Reforming;
            }

            if (lockReleased && distFromImpact >= cfg.CavalryReformDistanceFromAttackedFormation * 0.85f)
            {
                stateReason = "reform distance reached";
                return CavalryChargeState.ReformDistanceReached;
            }

            if (lockReleased)
            {
                stateReason = "disengaging / lock off";
                return CavalryChargeState.Disengaging;
            }

            if (rally != null && rally.RallyingTroops > rally.AbsorbableTroops && rally.TotalTroops > 0)
            {
                stateReason = "rallying to nucleus";
                return CavalryChargeState.RallyingToCommander;
            }

            if (rally != null && rally.TotalTroops > 0
                && rally.RallyingTroops + rally.AbsorbableTroops > rally.AssignedTroops + 1)
            {
                stateReason = "assembling";
                return CavalryChargeState.MountedFormationAssembling;
            }

            if (!horseArcherHeavy && shockHeavy && distTarget > cfg.CavalryReleaseLockDistance)
            {
                if (prev == CavalryChargeState.ChargeReady || prev == CavalryChargeState.Charging)
                {
                    stateReason = "charging continuation";
                    return CavalryChargeState.Charging;
                }

                stateReason = "charge ready";
                return CavalryChargeState.ChargeReady;
            }

            if (horseArcherHeavy)
            {
                stateReason = "horse archer skirmish line";
                return CavalryChargeState.MountedFormationAssembling;
            }

            stateReason = "default mounted";
            return CavalryChargeState.MountedFormationAssembling;
        }

        private static bool IsCommanderAgentValid(CommanderPresenceResult presence)
        {
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

        private static Formation FindNearestEnemyFormation(
            TaleWorlds.MountAndBlade.Mission mission,
            Formation source,
            FormationDataAdapter adapter)
        {
            if (mission == null || source == null || adapter == null)
            {
                return null;
            }

            try
            {
                Team player = mission.PlayerTeam;
                if (player == null)
                {
                    return null;
                }

                FormationDataResult src = adapter.TryGetFormationCenter(source);
                if (!src.Success)
                {
                    return null;
                }

                float best = float.MaxValue;
                Formation bestF = null;
                foreach (Team team in mission.Teams)
                {
                    if (team == null || team == player)
                    {
                        continue;
                    }

                    if (!team.IsEnemyOf(player))
                    {
                        continue;
                    }

                    foreach (Formation f in team.FormationsIncludingEmpty)
                    {
                        if (f == null || f.CountOfUnits <= 0)
                        {
                            continue;
                        }

                        FormationDataResult c = adapter.TryGetFormationCenter(f);
                        if (!c.Success)
                        {
                            continue;
                        }

                        float d = PlanarDistance(src.Vec3, c.Vec3);
                        if (d < best)
                        {
                            best = d;
                            bestF = f;
                        }
                    }
                }

                return bestF;
            }
            catch
            {
                return null;
            }
        }

        private static Vec3? TryTargetPosition(Formation target, FormationDataAdapter adapter)
        {
            if (target == null || adapter == null)
            {
                return null;
            }

            try
            {
                FormationDataResult c = adapter.TryGetFormationCenter(target);
                return c.Success ? c.Vec3 : (Vec3?)null;
            }
            catch
            {
                return null;
            }
        }

        private static float ComputeDistanceToTarget(
            Formation source,
            Formation target,
            Vec3? targetPosFallback,
            FormationDataAdapter adapter)
        {
            try
            {
                FormationDataResult s = adapter.TryGetFormationCenter(source);
                if (!s.Success)
                {
                    return float.MaxValue;
                }

                if (target != null)
                {
                    FormationDataResult t = adapter.TryGetFormationCenter(target);
                    if (t.Success)
                    {
                        return PlanarDistance(s.Vec3, t.Vec3);
                    }
                }

                if (targetPosFallback.HasValue)
                {
                    return PlanarDistance(s.Vec3, targetPosFallback.Value);
                }
            }
            catch
            {
                // fall through
            }

            return float.MaxValue;
        }

        private static float ComputeDistanceFromImpact(
            Formation source,
            Vec3? impact,
            FormationDataAdapter adapter,
            float fallbackDistanceToTarget)
        {
            try
            {
                if (!impact.HasValue)
                {
                    return fallbackDistanceToTarget;
                }

                FormationDataResult s = adapter.TryGetFormationCenter(source);
                if (!s.Success)
                {
                    return fallbackDistanceToTarget;
                }

                return PlanarDistance(s.Vec3, impact.Value);
            }
            catch
            {
                return fallbackDistanceToTarget;
            }
        }

        private static float ComputeNearestEnemyFormationDistance(
            TaleWorlds.MountAndBlade.Mission mission,
            Formation source,
            FormationDataAdapter adapter,
            out bool known)
        {
            known = false;
            if (mission == null || source == null || adapter == null)
            {
                return float.MaxValue;
            }

            try
            {
                Team player = mission.PlayerTeam;
                if (player == null)
                {
                    return float.MaxValue;
                }

                FormationDataResult src = adapter.TryGetFormationCenter(source);
                if (!src.Success)
                {
                    return float.MaxValue;
                }

                float best = float.MaxValue;
                foreach (Team team in mission.Teams)
                {
                    if (team == null || team == player || !team.IsEnemyOf(player))
                    {
                        continue;
                    }

                    foreach (Formation f in team.FormationsIncludingEmpty)
                    {
                        if (f == null || f.CountOfUnits <= 0)
                        {
                            continue;
                        }

                        FormationDataResult c = adapter.TryGetFormationCenter(f);
                        if (!c.Success)
                        {
                            continue;
                        }

                        float d = PlanarDistance(src.Vec3, c.Vec3);
                        if (d < best)
                        {
                            best = d;
                        }
                    }
                }

                known = best < float.MaxValue * 0.5f;
                return best;
            }
            catch
            {
                return float.MaxValue;
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
