using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;

namespace Bannerlord.RTSCameraLite.Mission
{
    /// <summary>
    /// Decides whether the RTS commander shell may attach. Conservative: unknown or ambiguous missions are unsupported.
    /// </summary>
    public static class CommanderMissionModeGate
    {
        /// <summary>
        /// Returns true only for battle or deployment phases (field/siege/custom battle style).
        /// Tournament, duel, conversation, cutscene, replay, benchmark, stealth, barter, and startup are excluded via
        /// <see cref="TaleWorlds.Core.MissionMode"/> allow-listing.
        /// </summary>
        public static bool IsSupportedMission(TaleWorlds.MountAndBlade.Mission mission)
        {
            if (mission == null || mission.MissionEnded)
            {
                return false;
            }

            MissionMode mode = mission.Mode;
            return mode == MissionMode.Battle || mode == MissionMode.Deployment;
        }
    }
}
