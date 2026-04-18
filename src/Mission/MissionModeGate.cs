using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;

namespace Bannerlord.RTSCameraLite.Mission
{
    internal static class MissionModeGate
    {
        public static bool IsSupportedMission(TaleWorlds.MountAndBlade.Mission mission)
        {
            if (mission == null)
            {
                return false;
            }

            return mission.Mode == MissionMode.Battle
                || mission.Mode == MissionMode.Deployment;
        }
    }
}
