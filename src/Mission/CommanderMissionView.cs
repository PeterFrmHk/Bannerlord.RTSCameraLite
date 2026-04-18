using Bannerlord.RTSCameraLite.Adapters;
using Bannerlord.RTSCameraLite.Camera;
using Bannerlord.RTSCameraLite.Commander;
using Bannerlord.RTSCameraLite.Config;
using Bannerlord.RTSCameraLite.Core;
using Bannerlord.RTSCameraLite.Doctrine;
using Bannerlord.RTSCameraLite.Input;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.View.MissionViews;

namespace Bannerlord.RTSCameraLite.Mission
{
    /// <summary>
    /// Mission shell: commander mode, camera pose, input guards, presence/doctrine/eligibility scans (Slice 8+), commander anchor (Slice 9).
    /// </summary>
    public sealed class CommanderMissionView : MissionView
    {
        private readonly CommanderModeState _commanderModeState = new CommanderModeState();
        private readonly CommanderInputReader _commanderInput = new CommanderInputReader();
        private readonly CameraBridge _cameraBridge = new CameraBridge();
        private readonly CommanderCameraController _cameraController = new CommanderCameraController();
        private readonly BackspaceConflictGuard _backspaceConflictGuard = new BackspaceConflictGuard();
        private readonly CommanderNativeInputGuard _nativeInputGuard = new CommanderNativeInputGuard();
        private readonly FormationDataAdapter _formationDataAdapter = new FormationDataAdapter();
        private CommanderAssignmentService _commanderAssignmentService;
        private FormationEligibilityRules _formationEligibilityRules;
        private float _commanderPresenceScanTimer;
        private const float CommanderPresenceScanIntervalSeconds = 2.5f;
        private CommanderConfig _commanderConfig = CommanderConfigDefaults.CreateDefault();
        private bool _loggedShellActive;
        private bool _lastLoggedEnabled;
        private bool _hasLoggedEnabledState;
        private bool _lifecycleCleanupDone;
        private bool _loggedFirstInternalPose;
        private bool _loggedCameraBridgeNotAppliedWarning;
        private CommanderAnchorResolver _anchorResolver;
        private CommanderAnchorSettings _anchorSettings;
        private float _anchorScanAccum;
        private const float CommanderAnchorScanIntervalSeconds = 2f;

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
            _backspaceConflictGuard.ApplyConfig(_commanderConfig);
            _nativeInputGuard.ApplyConfig(_commanderConfig);
            _cameraController.ApplyMovementSettings(CommanderCameraMovementSettings.FromConfig(_commanderConfig));
            _cameraController.InitializeFromMission(Mission);

            if (_commanderConfig.StartBattlesInCommanderMode)
            {
                _commanderModeState.Enable(ModConstants.CommanderShellDefaultEnableReason);
                SyncCommanderInputGuardsWithCommanderMode();
            }
            else
            {
                _commanderModeState.Disable("config: StartBattlesInCommanderMode false");
                SyncCommanderInputGuardsWithCommanderMode();
            }

            ModLogger.LogDebug(
                $"{ModConstants.ModuleId}: commander config — loaded={loadResult.Loaded}, usedDefaults={loadResult.UsedDefaults}, createdFile={loadResult.CreatedDefaultFile}: {loadResult.Message}");
            _commanderAssignmentService = new CommanderAssignmentService(_formationDataAdapter);
            _commanderAssignmentService.ApplyDetectionSettings(CommanderDetectionSettings.FromConfig(_commanderConfig));
            _formationEligibilityRules = new FormationEligibilityRules(FormationEligibilitySettings.FromConfig(_commanderConfig));
            _anchorResolver = new CommanderAnchorResolver(_formationDataAdapter);
            _anchorSettings = CommanderAnchorSettings.FromConfig(_commanderConfig);
            _anchorScanAccum = CommanderAnchorScanIntervalSeconds;
            _commanderPresenceScanTimer = CommanderPresenceScanIntervalSeconds;
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

            MaybeScanFriendlyFormationCommanders(dt);

            bool toggle = _commanderInput.TryConsumeCommanderModeToggle(Input);
            if (!toggle)
            {
                toggle = _commanderInput.TryConsumeEmergencyDebugCommanderToggle(Input);
            }

            if (toggle)
            {
                _commanderModeState.Toggle("commander mode toggle key");
                SyncCommanderInputGuardsWithCommanderMode();
                LogEnabledTransition();
                if (_commanderModeState.IsEnabled)
                {
                    _cameraController.ApplyMovementSettings(CommanderCameraMovementSettings.FromConfig(_commanderConfig));
                    _cameraController.InitializeFromMission(Mission);
                }
                else
                {
                    _loggedFirstInternalPose = false;
                }
            }

            _backspaceConflictGuard.Tick();
            _nativeInputGuard.Tick();
            TickCommanderCameraAndBridge(dt);
            if (_commanderModeState.IsEnabled)
            {
                MaybeScanCommanderAnchors(dt);
            }
        }

        public override void OnRemoveBehavior()
        {
            EnsureLifecycleCleanup("behavior removed");
            base.OnRemoveBehavior();
        }

        private void MaybeScanFriendlyFormationCommanders(float dt)
        {
            if (_commanderAssignmentService == null || Mission?.PlayerTeam == null)
            {
                return;
            }

            _commanderPresenceScanTimer += dt;
            if (_commanderPresenceScanTimer < CommanderPresenceScanIntervalSeconds)
            {
                return;
            }

            _commanderPresenceScanTimer = 0f;

            int total = 0;
            int commanded = 0;
            try
            {
                foreach (Formation formation in Mission.PlayerTeam.FormationsIncludingEmpty)
                {
                    if (formation == null || formation.CountOfUnits <= 0)
                    {
                        continue;
                    }

                    total++;
                    CommanderPresenceResult result = _commanderAssignmentService.DetectCommander(Mission, formation);
                    if (result.HasCommander)
                    {
                        commanded++;
                    }

                    if (_commanderConfig.EnableEligibilityDebug && _formationEligibilityRules != null)
                    {
                        FormationDoctrineProfile doctrine = DoctrineScoreCalculator.Compute(formation, result, _formationDataAdapter);
                        FormationEligibilityResult elig = _formationEligibilityRules.Evaluate(formation, result, doctrine);
                        string label = formation.RepresentativeClass.ToString();
                        ModLogger.LogDebug(
                            $"{ModConstants.ModuleId}: Eligibility: {label} allowed {FormatEligibilityTypes(elig.AllowedFormationTypes)}; denied {FormatEligibilityTypes(elig.DeniedFormationTypes)}.");
                    }
                }
            }
            catch (System.Exception ex)
            {
                ModLogger.LogDebug($"{ModConstants.ModuleId}: Commander scan skipped ({ex.Message})");
                return;
            }

            ModLogger.LogDebug($"{ModConstants.ModuleId}: Commander scan: {commanded}/{total} formations commanded");
        }

        private static string FormatEligibilityTypes(System.Collections.Generic.List<AllowedFormationType> types)
        {
            if (types == null || types.Count == 0)
            {
                return "(none)";
            }

            return string.Join(", ", types);
        }

        private void MaybeScanCommanderAnchors(float dt)
        {
            if (_anchorResolver == null || _commanderAssignmentService == null || Mission == null)
            {
                return;
            }

            _anchorScanAccum += dt;
            if (_anchorScanAccum < CommanderAnchorScanIntervalSeconds)
            {
                return;
            }

            _anchorScanAccum = 0f;

            Team playerTeam = Mission.PlayerTeam;
            if (playerTeam == null)
            {
                return;
            }

            int inside = 0;
            int outside = 0;
            int noAnchor = 0;

            try
            {
                foreach (Formation formation in playerTeam.FormationsIncludingEmpty)
                {
                    if (formation == null || formation.CountOfUnits <= 0)
                    {
                        continue;
                    }

                    CommanderPresenceResult presence = _commanderAssignmentService.DetectCommander(Mission, formation);
                    CommanderAnchorState anchor = _anchorResolver.ResolveAnchor(Mission, formation, presence, _anchorSettings);
                    if (!anchor.HasAnchor)
                    {
                        noAnchor++;
                        continue;
                    }

                    if (anchor.CommanderInsideAnchorZone)
                    {
                        inside++;
                    }
                    else
                    {
                        outside++;
                    }
                }
            }
            catch (System.Exception ex)
            {
                ModLogger.LogDebug($"{ModConstants.ModuleId}: commander anchor scan skipped ({ex.Message})");
                return;
            }

            if (_commanderConfig.EnableCommanderAnchorDebug)
            {
                ModLogger.LogDebug(
                    $"{ModConstants.ModuleId}: Commander anchors: {inside} inside, {outside} out of position, {noAnchor} without anchor.");
            }
        }

        private void SyncCommanderInputGuardsWithCommanderMode()
        {
            if (_commanderModeState.IsEnabled)
            {
                _backspaceConflictGuard.EnterCommanderMode();
                _nativeInputGuard.EnterCommanderMode();
            }
            else
            {
                _backspaceConflictGuard.ExitCommanderMode();
                _nativeInputGuard.ExitCommanderMode();
                RestoreNativeCameraAfterCommanderDisable();
            }
        }

        private void RestoreNativeCameraAfterCommanderDisable()
        {
            if (Mission == null)
            {
                return;
            }

            CameraBridgeResult restore = _cameraBridge.RestoreNativeCamera(Mission);
            if (!restore.Restored)
            {
                ModLogger.LogDebug(
                    $"{ModConstants.ModuleId}: CameraBridge restore (commander off) — Restored={restore.Restored}: {restore.Message}");
            }
        }

        private void TickCommanderCameraAndBridge(float dt)
        {
            if (!_commanderModeState.IsEnabled || Mission == null)
            {
                return;
            }

            if (!_cameraController.HasPose)
            {
                _cameraController.InitializeFromMission(Mission);
            }

            if (!_cameraController.HasPose)
            {
                return;
            }

            MaybeLogFirstInternalPose();
            CommanderInputSnapshot snapshot = _commanderInput.ReadCameraSnapshot(Input);
            _cameraController.Tick(snapshot, dt);

            CameraBridgeResult apply = _cameraBridge.TryApply(Mission, _cameraController.GetPose());
            if (!apply.Applied && !_loggedCameraBridgeNotAppliedWarning)
            {
                _loggedCameraBridgeNotAppliedWarning = true;
                ModLogger.Warn(
                    $"{ModConstants.ModuleId}: CameraBridge did not apply (engine path not wired this slice). Applied={apply.Applied}, Restored={apply.Restored}: {apply.Message}");
            }
        }

        private void MaybeLogFirstInternalPose()
        {
            if (_loggedFirstInternalPose)
            {
                return;
            }

            _loggedFirstInternalPose = true;
            CommanderCameraPose pose = _cameraController.GetPose();
            ModLogger.LogDebug(
                $"{ModConstants.ModuleId}: internal commander camera pose initialized — pos=({pose.Position.x:F1},{pose.Position.y:F1},{pose.Position.z:F1}), yaw={pose.Yaw:F3} rad, pitch={pose.Pitch:F1}°, h={pose.Height:F1}");
        }

        private void EnsureLifecycleCleanup(string reason)
        {
            if (_lifecycleCleanupDone)
            {
                return;
            }

            _lifecycleCleanupDone = true;
            _nativeInputGuard.Cleanup();
            _backspaceConflictGuard.Cleanup();
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
            _loggedFirstInternalPose = false;
            _loggedCameraBridgeNotAppliedWarning = false;
            _commanderPresenceScanTimer = 0f;
            _anchorScanAccum = 0f;
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
