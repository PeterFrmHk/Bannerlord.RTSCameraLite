using Bannerlord.RTSCameraLite.Adapters;
using Bannerlord.RTSCameraLite.Camera;
using Bannerlord.RTSCameraLite.Config;
using Bannerlord.RTSCameraLite.Core;
using Bannerlord.RTSCameraLite.Input;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.View.MissionViews;

namespace Bannerlord.RTSCameraLite.Mission
{
    /// <summary>
    /// Mission shell: commander mode, camera bridge probe, file-backed config (Slice 6).
    /// </summary>
    public sealed class CommanderMissionView : MissionView
    {
        private readonly CommanderModeState _commanderModeState = new CommanderModeState();
        private readonly CommanderInputReader _commanderInput = new CommanderInputReader();
        private readonly CameraBridge _cameraBridge = new CameraBridge();
        private readonly CommanderCameraController _cameraController = new CommanderCameraController();
        private readonly BackspaceConflictGuard _backspaceConflictGuard = new BackspaceConflictGuard();
        private CommanderConfig _commanderConfig = CommanderConfigDefaults.CreateDefault();
        private bool _loggedShellActive;
        private bool _lastLoggedEnabled;
        private bool _hasLoggedEnabledState;
        private bool _lifecycleCleanupDone;
        private bool _cameraBridgeProbeCompleted;

        public CommanderModeState CommanderModeState => _commanderModeState;

        public override void OnBehaviorInitialize()
        {
            base.OnBehaviorInitialize();

            if (!CommanderMissionModeGate.IsSupportedMission(Mission))
            {
                ModLogger.LogDebug(
                    $"{ModConstants.ModuleId}: CommanderMissionView initialized on unsupported mission; commander mode stays off.");
                return;
            }

            ConfigLoadResult loadResult = CommanderConfigService.LoadOrCreate();
            _commanderConfig = loadResult.Config ?? CommanderConfigDefaults.CreateDefault();
            _commanderInput.ApplyConfig(_commanderConfig);
            _cameraController.ApplyMovementSettings(CommanderCameraMovementSettings.FromConfig(_commanderConfig));
            _cameraController.InitializeFromMission(Mission);

            if (_commanderConfig.StartBattlesInCommanderMode)
            {
                _commanderModeState.Enable(ModConstants.CommanderShellDefaultEnableReason);
                _backspaceConflictGuard.EnterCommanderMode();
            }
            else
            {
                _commanderModeState.Disable("config: StartBattlesInCommanderMode false");
                _backspaceConflictGuard.ExitCommanderMode();
            }

            ModLogger.LogDebug(
                $"{ModConstants.ModuleId}: commander config — loaded={loadResult.Loaded}, usedDefaults={loadResult.UsedDefaults}, createdFile={loadResult.CreatedDefaultFile}: {loadResult.Message}");
            LogShellActiveOnce();
            LogEnabledTransition();
        }

        public override void OnMissionTick(float dt)
        {
            base.OnMissionTick(dt);

            if (Mission != null && Mission.MissionEnded)
            {
                EnsureLifecycleCleanup("mission ended");
                return;
            }

            if (!CommanderMissionModeGate.IsSupportedMission(Mission))
            {
                return;
            }

            bool toggle = _commanderInput.TryConsumeCommanderModeToggle(Input);
            if (!toggle)
            {
                toggle = _commanderInput.TryConsumeEmergencyDebugCommanderToggle(Input);
            }

            if (toggle)
            {
                _commanderModeState.Toggle("commander mode toggle key");
                SyncBackspaceGuardWithCommanderMode();
                LogEnabledTransition();
                if (_commanderModeState.IsEnabled)
                {
                    _cameraController.ApplyMovementSettings(CommanderCameraMovementSettings.FromConfig(_commanderConfig));
                    _cameraController.InitializeFromMission(Mission);
                }
            }

            _backspaceConflictGuard.Tick();
            MaybeProbeCameraBridgeOnce();
        }

        public override void OnRemoveBehavior()
        {
            EnsureLifecycleCleanup("behavior removed");
            base.OnRemoveBehavior();
        }

        private void SyncBackspaceGuardWithCommanderMode()
        {
            if (_commanderModeState.IsEnabled)
            {
                _backspaceConflictGuard.EnterCommanderMode();
            }
            else
            {
                _backspaceConflictGuard.ExitCommanderMode();
            }
        }

        private void MaybeProbeCameraBridgeOnce()
        {
            if (!_commanderModeState.IsEnabled || _cameraBridgeProbeCompleted)
            {
                return;
            }

            _cameraBridgeProbeCompleted = true;
            CameraBridgeResult result = _cameraBridge.TryApply(Mission, _cameraController.GetPose());
            if (!result.Applied)
            {
                ModLogger.LogDebug(
                    $"{ModConstants.ModuleId}: CameraBridge probe — Applied={result.Applied}, Restored={result.Restored}: {result.Message}");
            }
        }

        private void EnsureLifecycleCleanup(string reason)
        {
            if (_lifecycleCleanupDone)
            {
                return;
            }

            _lifecycleCleanupDone = true;
            _backspaceConflictGuard.ExitCommanderMode();
            if (Mission != null)
            {
                CameraBridgeResult restore = _cameraBridge.RestoreNativeCamera(Mission);
                if (!restore.Restored)
                {
                    ModLogger.LogDebug(
                        $"{ModConstants.ModuleId}: CameraBridge restore — Restored={restore.Restored}: {restore.Message}");
                }
            }

            _commanderModeState.ForceDisabled(reason);
            ModLogger.LogDebug($"{ModConstants.ModuleId}: RTS Commander Mode cleanup ({reason}).");
            _loggedShellActive = false;
            _hasLoggedEnabledState = false;
            _cameraBridgeProbeCompleted = false;
            _cameraController.Reset();
        }

        private void LogShellActiveOnce()
        {
            if (_loggedShellActive)
            {
                return;
            }

            _loggedShellActive = true;
            ModLogger.LogDebug($"{ModConstants.ModuleId}: RTS Commander Mode active");
            ModLogger.Info("RTS Commander Mode active");
        }

        private void LogEnabledTransition()
        {
            if (_hasLoggedEnabledState && _lastLoggedEnabled == _commanderModeState.IsEnabled)
            {
                return;
            }

            _hasLoggedEnabledState = true;
            _lastLoggedEnabled = _commanderModeState.IsEnabled;
            ModLogger.LogDebug(
                $"{ModConstants.ModuleId}: RTS Commander Mode {(_commanderModeState.IsEnabled ? "ENABLED" : "DISABLED")} (toggles={_commanderModeState.ToggleCount}, reason={_commanderModeState.LastToggleReason})");
        }
    }
}
