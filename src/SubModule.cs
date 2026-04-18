using Bannerlord.RTSCameraLite.Config;
using Bannerlord.RTSCameraLite.Core;
using Bannerlord.RTSCameraLite.Mission;
using TaleWorlds.MountAndBlade;

namespace Bannerlord.RTSCameraLite
{
    /// <summary>
    /// Module entry: crash quarantine keeps mission runtime dormant unless explicitly enabled.
    /// </summary>
    public sealed class SubModule : MBSubModuleBase
    {
        protected override void OnSubModuleLoad()
        {
            try
            {
                base.OnSubModuleLoad();
                ModLogger.LogDebug(
                    $"{ModConstants.ModuleId} loaded. Version {ModConstants.Version} ({ModConstants.LegacyShortName} codebase).");
            }
            catch (System.Exception ex)
            {
                ModLogger.Warn($"{ModConstants.ModuleId}: OnSubModuleLoad guarded failure ({ex})");
            }
        }

        protected override void OnBeforeInitialModuleScreenSetAsRoot()
        {
            try
            {
                base.OnBeforeInitialModuleScreenSetAsRoot();
                ModLogger.MarkUiReady();
                ModLogger.LogDebug($"{ModConstants.ModuleId} UI ready.");
                ModLogger.SafeStartupLog(
                    $"{ModConstants.DisplayName} v{ModConstants.Version} - foundation active (mission hooks opt-in).");
            }
            catch (System.Exception ex)
            {
                ModLogger.Warn($"{ModConstants.ModuleId}: OnBeforeInitialModuleScreenSetAsRoot guarded failure ({ex})");
            }
        }

        public override void OnMissionBehaviorInitialize(TaleWorlds.MountAndBlade.Mission mission)
        {
            try
            {
                base.OnMissionBehaviorInitialize(mission);

                if (!CommanderMissionModeGate.IsSupportedMission(mission))
                {
                    return;
                }

                if (!CommanderConfigService.TryReadMissionRuntimeHooksEnabledFailClosed())
                {
                    ModLogger.LogDebug($"{ModConstants.ModuleId}: mission runtime hooks disabled; CommanderMissionView not attached.");
                    return;
                }

                mission.AddMissionBehavior(new CommanderMissionView());
            }
            catch (System.Exception ex)
            {
                ModLogger.Warn($"{ModConstants.ModuleId}: OnMissionBehaviorInitialize guarded failure ({ex})");
            }
        }
    }
}
