using Bannerlord.RTSCameraLite.Core;
using TaleWorlds.MountAndBlade;

namespace Bannerlord.RTSCameraLite
{
    /// <summary>
    /// Slice 1: loadable module entry only. No mission behaviors registered yet.
    /// </summary>
    public sealed class SubModule : MBSubModuleBase
    {
        protected override void OnSubModuleLoad()
        {
            base.OnSubModuleLoad();

            ModLogger.LogDebug(
                $"{ModConstants.ModuleId} loaded. Version {ModConstants.Version} ({ModConstants.LegacyShortName} codebase)."
            );
        }

        protected override void OnBeforeInitialModuleScreenSetAsRoot()
        {
            base.OnBeforeInitialModuleScreenSetAsRoot();

            ModLogger.MarkUiReady();

            ModLogger.LogDebug($"{ModConstants.ModuleId} UI ready.");

            ModLogger.SafeStartupLog(
                $"{ModConstants.DisplayName} v{ModConstants.Version} — foundation active."
            );
        }

        public override void OnMissionBehaviorInitialize(Mission mission)
        {
            base.OnMissionBehaviorInitialize(mission);
            // Slice 1: intentionally no AddMissionBehavior — camera / doctrine ship in later slices.
        }
    }
}
