using Bannerlord.RTSCameraLite.Core;
using Bannerlord.RTSCameraLite.Mission;
using TaleWorlds.MountAndBlade;

namespace Bannerlord.RTSCameraLite
{
    public sealed class SubModule : MBSubModuleBase
    {
        protected override void OnSubModuleLoad()
        {
            base.OnSubModuleLoad();

            ModLogger.SafeStartupLog(
                $"{ModConstants.ModuleId} loaded. Version {ModConstants.Version}."
            );
        }

        protected override void OnBeforeInitialModuleScreenSetAsRoot()
        {
            base.OnBeforeInitialModuleScreenSetAsRoot();

            ModLogger.MarkUiReady();

            ModLogger.SafeStartupLog(
                $"{ModConstants.DisplayName} foundation initialized."
            );
        }

        public override void OnMissionBehaviorInitialize(TaleWorlds.MountAndBlade.Mission mission)
        {
            base.OnMissionBehaviorInitialize(mission);

            if (MissionModeGate.IsSupportedMission(mission))
            {
                mission.AddMissionBehavior(new RTSCameraMissionBehavior());
            }
        }
    }
}
