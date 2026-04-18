using Bannerlord.RTSCameraLite.Core;
using Bannerlord.RTSCameraLite.Input;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.View.MissionViews;

namespace Bannerlord.RTSCameraLite.Mission
{
    /// <summary>
    /// Slice 2 mission shell: attaches on supported battles, owns commander mode + input reader. No camera movement or native orders.
    /// </summary>
    public sealed class CommanderMissionView : MissionView
    {
        private readonly CommanderModeState _commanderModeState = new CommanderModeState();
        private readonly CommanderInputReader _commanderInput = new CommanderInputReader();
        private bool _loggedShellActive;
        private bool _lastLoggedEnabled;
        private bool _hasLoggedEnabledState;
        private bool _lifecycleCleanupDone;

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

            _commanderModeState.Enable(ModConstants.CommanderShellDefaultEnableReason);
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

            if (!toggle)
            {
                return;
            }

            _commanderModeState.Toggle("commander mode toggle key");
            LogEnabledTransition();
        }

        public override void OnRemoveBehavior()
        {
            EnsureLifecycleCleanup("behavior removed");
            base.OnRemoveBehavior();
        }

        private void EnsureLifecycleCleanup(string reason)
        {
            if (_lifecycleCleanupDone)
            {
                return;
            }

            _lifecycleCleanupDone = true;
            _commanderModeState.ForceDisabled(reason);
            ModLogger.LogDebug($"{ModConstants.ModuleId}: RTS Commander Mode cleanup ({reason}).");
            _loggedShellActive = false;
            _hasLoggedEnabledState = false;
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
