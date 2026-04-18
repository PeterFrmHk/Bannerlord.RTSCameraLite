using System.Collections.Generic;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace Bannerlord.RTSCameraLite.Adapters
{
    /// <summary>
    /// Read-only formation probes. Only APIs called here are those called out as public in Slice 0 research; everything else returns failure.
    /// </summary>
    public sealed class FormationDataAdapter
    {
        public FormationDataResult TryGetFormationCenter(Formation formation)
        {
            if (formation == null)
            {
                return FormationDataResult.Failure("formation is null");
            }

            try
            {
                if (formation.CountOfUnits <= 0)
                {
                    return FormationDataResult.Failure("formation has no units");
                }

                // 1.2.x reference: planar centers are Vec2; height comes from OrderGroundPosition (Vec3) when available.
                Vec2 planar = formation.SmoothedAverageUnitPosition;
                if (planar.LengthSquared < 1e-8f)
                {
                    planar = formation.CurrentPosition;
                }

                if (planar.LengthSquared < 1e-8f && formation.OrderPositionIsValid)
                {
                    planar = formation.OrderPosition;
                }

                Vec3 ground = formation.OrderGroundPosition;
                Vec3 center = new Vec3(planar.x, planar.y, ground.z);

                return new FormationDataResult(true, string.Empty, center);
            }
            catch
            {
                return FormationDataResult.Failure("CachedAveragePosition read threw");
            }
        }

        public FormationDataResult TryGetFormationFacing(Formation formation)
        {
            _ = formation;
            return FormationDataResult.Failure(
                "Slice 3: facing not wired — verify FacingOrder / formation direction accessors on pinned DLLs before reading.");
        }

        public FormationDataResult TryGetAgents(Formation formation)
        {
            if (formation == null)
            {
                return FormationDataResult.Failure("formation is null");
            }

            try
            {
                var list = new List<Agent>();
                formation.ApplyActionOnEachUnit(a =>
                {
                    if (a != null)
                    {
                        list.Add(a);
                    }
                });

                return new FormationDataResult(true, string.Empty, default, 0f, list);
            }
            catch
            {
                return FormationDataResult.Failure("Enumerate agents threw");
            }
        }

        public FormationDataResult TryDetectCommander(Formation formation)
        {
            _ = formation;
            return FormationDataResult.Failure(
                "Slice 3: commander detection not wired — map Team.GeneralAgent / Leader vs formation owner after mission-type matrix is defined.");
        }

        public FormationDataResult TryDetectMountedRatio(Formation formation)
        {
            if (formation == null)
            {
                return FormationDataResult.Failure("formation is null");
            }

            try
            {
                if (formation.CountOfUnits <= 0)
                {
                    return FormationDataResult.Failure("formation has no units");
                }

                if (!formation.HasAnyMountedUnit)
                {
                    return new FormationDataResult(true, string.Empty, default, 0f, null, false);
                }

                return FormationDataResult.Failure(
                    "Slice 3: mounted ratio not wired — HasAnyMountedUnit is coarse; per-agent mount scan deferred.");
            }
            catch
            {
                return FormationDataResult.Failure("mounted probe threw");
            }
        }
    }
}
