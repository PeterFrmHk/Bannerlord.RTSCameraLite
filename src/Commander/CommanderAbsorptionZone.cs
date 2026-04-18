using TaleWorlds.Library;

namespace Bannerlord.RTSCameraLite.Commander
{
    /// <summary>
    /// Spatial bands around the commander rally nucleus (Slice 12 — planar XY distance).
    /// </summary>
    public static class CommanderAbsorptionZone
    {
        public enum AbsorptionBand
        {
            InsideAbsorption = 0,
            Rallying = 1,
            OutsideRallyWithinCohesion = 2,
            OutsideCohesion = 3
        }

        public static AbsorptionBand Classify(Vec3 rallyPoint, Vec3 agentPosition, CommanderRallySettings settings)
        {
            if (settings == null)
            {
                return AbsorptionBand.OutsideCohesion;
            }

            float d = PlanarDistance(rallyPoint, agentPosition);
            if (d > settings.CohesionBreakRadius)
            {
                return AbsorptionBand.OutsideCohesion;
            }

            if (d <= settings.CommanderAbsorptionRadius)
            {
                return AbsorptionBand.InsideAbsorption;
            }

            if (d <= settings.CommanderRallyRadius)
            {
                return AbsorptionBand.Rallying;
            }

            return AbsorptionBand.OutsideRallyWithinCohesion;
        }

        public static float PlanarDistance(Vec3 a, Vec3 b)
        {
            float dx = a.x - b.x;
            float dy = a.y - b.y;
            return (float)System.Math.Sqrt(dx * dx + dy * dy);
        }
    }
}
