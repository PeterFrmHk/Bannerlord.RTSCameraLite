using Bannerlord.RTSCameraLite.Adapters;
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
        private HarmonyPatchService _harmonyPatchService;

        protected override void OnSubModuleLoad()
        {
            try
            {
                base.OnSubModuleLoad();
                ModLogger.LogDebug(
                    $"{ModConstants.ModuleId} loaded. Version {ModConstants.Version} ({ModConstants.LegacyShortName} codebase).");

                TryApplyHarmonyScaffoldFailClosed();
            }
            catch (System.Exception ex)
            {
                ModLogger.Warn($"{ModConstants.ModuleId}: OnSubModuleLoad guarded failure ({ex})");
            }
        }

        protected override void OnSubModuleUnloaded()
        {
            try
            {
                _harmonyPatchService?.UnpatchAll();
                _harmonyPatchService = null;
            }
            catch (System.Exception ex)
            {
                ModLogger.Warn($"{ModConstants.ModuleId}: OnSubModuleUnloaded Harmony cleanup ({ex})");
            }

            base.OnSubModuleUnloaded();
        }

        /// <summary>
        /// Harmony is compile-time + optional runtime scaffold only: gated by config and requires mission hooks opt-in.
        /// </summary>
        private void TryApplyHarmonyScaffoldFailClosed()
        {
            try
            {
                ConfigLoadResult load = CommanderConfigService.LoadOrCreate();
                CommanderConfig cfg = load.Config;
                if (!cfg.EnableHarmonyPatches || !cfg.EnableMissionRuntimeHooks)
                {
                    return;
                }

                _harmonyPatchService = new HarmonyPatchService();
                _harmonyPatchService.ApplyPatches(cfg.EnableHarmonyDiagnostics);
            }
            catch (System.Exception ex)
            {
                ModLogger.Warn($"{ModConstants.ModuleId}: Harmony scaffold skipped ({ex.Message})");
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
