using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace Bannerlord.RTSCameraLite.Tactical
{
    /// <summary>
    /// Resolves a world position to focus the RTS camera on a formation without issuing orders.
    /// </summary>
    internal static class FormationFocusController
    {
        public static bool TryGetFocusWorldPosition(
            TaleWorlds.MountAndBlade.Mission mission,
            Formation formation,
            out Vec3 worldPosition)
        {
            worldPosition = Vec3.Zero;

            if (mission == null || formation == null)
            {
                return false;
            }

            if (TryMedianAgentPosition(formation, out Vec3 median))
            {
                worldPosition = median;
                return true;
            }

            if (TryCaptainPosition(formation, out Vec3 captain))
            {
                worldPosition = captain;
                return true;
            }

            if (TryOrderGroundPosition(mission, formation, out Vec3 order))
            {
                worldPosition = order;
                return true;
            }

            return false;
        }

        private static bool TryMedianAgentPosition(Formation formation, out Vec3 position)
        {
            position = Vec3.Zero;
            try
            {
                Vec2 average = formation.OrderLocalAveragePosition;
                Agent agent = formation.GetMedianAgent(true, false, average);
                if (agent == null || !agent.IsActive())
                {
                    return false;
                }

                position = agent.Position;
                return true;
            }
            catch
            {
                return false;
            }
        }

        private static bool TryCaptainPosition(Formation formation, out Vec3 position)
        {
            position = Vec3.Zero;
            try
            {
                Agent captain = formation.Captain;
                if (captain == null || !captain.IsActive())
                {
                    return false;
                }

                position = captain.Position;
                return true;
            }
            catch
            {
                return false;
            }
        }

        private static bool TryOrderGroundPosition(TaleWorlds.MountAndBlade.Mission mission, Formation formation, out Vec3 position)
        {
            position = Vec3.Zero;
            try
            {
                Vec2 order = formation.OrderPosition;
                float referenceZ = mission.MainAgent != null ? mission.MainAgent.Position.z : 0f;
                if (formation.Captain != null && formation.Captain.IsActive())
                {
                    referenceZ = formation.Captain.Position.z;
                }

                position = new Vec3(order.x, order.y, referenceZ);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
