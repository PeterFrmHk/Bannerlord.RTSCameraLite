using System;
using Bannerlord.RTSCameraLite.Adapters;
using Bannerlord.RTSCameraLite.Commands;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace Bannerlord.RTSCameraLite.Doctrine
{
    /// <summary>Resolves cavalry sequence targets and distances with safe fallbacks (Slice 16).</summary>
    public static class CavalryTargetTracker
    {
        public static bool TryGetNearestEnemyFormation(
            TaleWorlds.MountAndBlade.Mission mission,
            Formation sourceFormation,
            FormationDataAdapter adapter,
            out Formation enemyFormation)
        {
            enemyFormation = FindNearestEnemyFormation(mission, sourceFormation, adapter);
            return enemyFormation != null;
        }

        public static bool TryResolveTargetFormation(CommandIntent intent, out Formation targetFormation)
        {
            targetFormation = null;
            try
            {
                if (intent?.TargetFormation != null && intent.TargetFormation.CountOfUnits > 0)
                {
                    targetFormation = intent.TargetFormation;
                    return true;
                }

                return false;
            }
            catch
            {
                return false;
            }
        }

        public static bool TryResolveTargetPosition(
            TaleWorlds.MountAndBlade.Mission mission,
            FormationDataAdapter adapter,
            Formation sourceFormation,
            CommandIntent intent,
            Formation resolvedTargetFormation,
            Vec3? lastKnownImpactOrTarget,
            out Vec3 targetWorld)
        {
            targetWorld = Vec3.Zero;
            try
            {
                if (intent?.TargetPosition.HasValue == true && IsFiniteVec3(intent.TargetPosition.Value))
                {
                    targetWorld = intent.TargetPosition.Value;
                    return true;
                }

                if (resolvedTargetFormation != null && adapter != null)
                {
                    FormationDataResult c = adapter.TryGetFormationCenter(resolvedTargetFormation);
                    if (c.Success)
                    {
                        targetWorld = c.Vec3;
                        return true;
                    }
                }

                if (lastKnownImpactOrTarget.HasValue && IsFiniteVec3(lastKnownImpactOrTarget.Value))
                {
                    targetWorld = lastKnownImpactOrTarget.Value;
                    return true;
                }

                if (sourceFormation != null && adapter != null)
                {
                    Formation nearest = FindNearestEnemyFormation(mission, sourceFormation, adapter);
                    if (nearest != null)
                    {
                        FormationDataResult c = adapter.TryGetFormationCenter(nearest);
                        if (c.Success)
                        {
                            targetWorld = c.Vec3;
                            return true;
                        }
                    }
                }

                return false;
            }
            catch
            {
                return false;
            }
        }

        public static bool TryComputeDistanceToTarget(
            Formation sourceFormation,
            Formation targetFormation,
            Vec3 targetWorld,
            FormationDataAdapter adapter,
            out float distance)
        {
            distance = float.MaxValue;
            if (sourceFormation == null || adapter == null)
            {
                return false;
            }

            try
            {
                FormationDataResult s = adapter.TryGetFormationCenter(sourceFormation);
                if (!s.Success)
                {
                    return false;
                }

                if (targetFormation != null)
                {
                    FormationDataResult t = adapter.TryGetFormationCenter(targetFormation);
                    if (t.Success)
                    {
                        distance = PlanarDistance(s.Vec3, t.Vec3);
                        return true;
                    }
                }

                distance = PlanarDistance(s.Vec3, targetWorld);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static bool TryComputeDistanceFromImpact(
            Formation sourceFormation,
            Vec3? impactPosition,
            Vec3 targetWorldFallback,
            FormationDataAdapter adapter,
            out float distance)
        {
            distance = float.MaxValue;
            if (sourceFormation == null || adapter == null)
            {
                return false;
            }

            try
            {
                FormationDataResult s = adapter.TryGetFormationCenter(sourceFormation);
                if (!s.Success)
                {
                    return false;
                }

                if (impactPosition.HasValue && IsFiniteVec3(impactPosition.Value))
                {
                    distance = PlanarDistance(s.Vec3, impactPosition.Value);
                    return true;
                }

                distance = PlanarDistance(s.Vec3, targetWorldFallback);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static bool TryComputeNearestEnemyFormationDistance(
            TaleWorlds.MountAndBlade.Mission mission,
            Formation sourceFormation,
            FormationDataAdapter adapter,
            out float distance,
            out bool known)
        {
            distance = float.MaxValue;
            known = false;
            if (mission == null || sourceFormation == null || adapter == null)
            {
                return false;
            }

            try
            {
                Team player = mission.PlayerTeam;
                if (player == null)
                {
                    return false;
                }

                FormationDataResult src = adapter.TryGetFormationCenter(sourceFormation);
                if (!src.Success)
                {
                    return false;
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
                distance = best;
                return known;
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

        private static float PlanarDistance(Vec3 a, Vec3 b)
        {
            float dx = a.x - b.x;
            float dy = a.y - b.y;
            return (float)Math.Sqrt(dx * dx + dy * dy);
        }

        private static bool IsFiniteVec3(Vec3 v)
        {
            return !(float.IsNaN(v.x) || float.IsInfinity(v.x)
                || float.IsNaN(v.y) || float.IsInfinity(v.y)
                || float.IsNaN(v.z) || float.IsInfinity(v.z));
        }
    }
}
