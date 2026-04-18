using System.Collections.Generic;
using Bannerlord.RTSCameraLite.Adapters;
using TaleWorlds.Core;
using Bannerlord.RTSCameraLite.Camera;
using Bannerlord.RTSCameraLite.Commander;
using Bannerlord.RTSCameraLite.Commands;
using Bannerlord.RTSCameraLite.Config;
using Bannerlord.RTSCameraLite.Core;
using Bannerlord.RTSCameraLite.Doctrine;
using Bannerlord.RTSCameraLite.Equipment;
using Bannerlord.RTSCameraLite.Input;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;
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
        private DoctrineScoreCalculator _doctrineScoreCalculator;
        private float _doctrineScanAccum;
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
        private CommanderRallyPlanner _commanderRallyPlanner;
        private TroopAbsorptionController _troopAbsorptionController;
        private RowRankSlotAssigner _rowRankSlotAssigner;
        private CommanderRallySettings _commanderRallySettings;
        private float _rallyScanAccum;
        private float _absorptionMissionTime;
        private CommandRouter _commandRouter;
        private float _secondsSinceCommandValidationLog;

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
            _doctrineScoreCalculator = new DoctrineScoreCalculator(_formationDataAdapter);
            _doctrineScanAccum = System.Math.Max(0.1f, _commanderConfig.DoctrineScanIntervalSeconds);
            _anchorResolver = new CommanderAnchorResolver(_formationDataAdapter);
            _anchorSettings = CommanderAnchorSettings.FromConfig(_commanderConfig);
            _anchorScanAccum = CommanderAnchorScanIntervalSeconds;
            _commanderRallyPlanner = new CommanderRallyPlanner(_formationDataAdapter);
            _troopAbsorptionController = new TroopAbsorptionController();
            _rowRankSlotAssigner = new RowRankSlotAssigner(_formationDataAdapter);
            _commanderRallySettings = CommanderRallySettings.FromConfig(_commanderConfig);
            _rallyScanAccum = System.Math.Max(0.1f, _commanderRallySettings.RallyScanIntervalSeconds);
            _commanderPresenceScanTimer = CommanderPresenceScanIntervalSeconds;
            _commandRouter = new CommandRouter(_commanderConfig, new NativeOrderPrimitiveExecutor());
            _secondsSinceCommandValidationLog = 0f;
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

            _absorptionMissionTime += dt;
            MaybeScanFriendlyFormationCommanders(dt);
            MaybeScanDoctrineProfiles(dt);
            MaybeScanRallyAbsorption(dt);

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
                MaybeDebugCommandRouterKeys(dt);
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

                    if (_commanderConfig.EnableEligibilityDebug && _formationEligibilityRules != null && _doctrineScoreCalculator != null)
                    {
                        DoctrineScoreResult doctrineRes = _doctrineScoreCalculator.Compute(
                            Mission,
                            formation,
                            result,
                            DoctrineScoreSettings.FromConfig(_commanderConfig));
                        if (!doctrineRes.ComputationSucceeded || doctrineRes.Profile == null)
                        {
                            continue;
                        }

                        FormationEligibilityResult elig = _formationEligibilityRules.Evaluate(formation, result, doctrineRes.Profile);
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

        /// <summary>
        /// Slice 12 — rally/absorption and slot planning only (no native orders, no forced movement).
        /// </summary>
        private void MaybeScanRallyAbsorption(float dt)
        {
            if (_commanderRallyPlanner == null
                || _troopAbsorptionController == null
                || _rowRankSlotAssigner == null
                || _commanderRallySettings == null
                || _formationEligibilityRules == null
                || _doctrineScoreCalculator == null
                || _commanderAssignmentService == null
                || Mission?.PlayerTeam == null)
            {
                return;
            }

            float interval = System.Math.Max(0.5f, _commanderRallySettings.RallyScanIntervalSeconds);
            _rallyScanAccum += dt;
            if (_rallyScanAccum < interval)
            {
                return;
            }

            _rallyScanAccum = 0f;
            DoctrineScoreSettings doctrineSettings = DoctrineScoreSettings.FromConfig(_commanderConfig);

            try
            {
                foreach (Formation formation in Mission.PlayerTeam.FormationsIncludingEmpty)
                {
                    if (formation == null || formation.CountOfUnits <= 0)
                    {
                        continue;
                    }

                    CommanderPresenceResult presence = _commanderAssignmentService.DetectCommander(Mission, formation);
                    CommanderAnchorState anchor = _anchorResolver != null
                        ? _anchorResolver.ResolveAnchor(Mission, formation, presence, _anchorSettings)
                        : CommanderAnchorState.None("anchor resolver null");
                    DoctrineScoreResult doctrineRes = _doctrineScoreCalculator.Compute(
                        Mission,
                        formation,
                        presence,
                        doctrineSettings);
                    if (!doctrineRes.ComputationSucceeded || doctrineRes.Profile == null)
                    {
                        continue;
                    }

                    FormationEligibilityResult elig = _formationEligibilityRules.Evaluate(formation, presence, doctrineRes.Profile);
                    FormationCompositionProfile comp = FormationCompositionAnalyzer.Analyze(_formationDataAdapter, formation);
                    RowRankSpacingPlan plan = FormationLayoutPlanner.Build(
                        formation,
                        doctrineRes.Profile,
                        elig,
                        comp,
                        presence);

                    CommanderRallyState rallyProbe = _commanderRallyPlanner.BuildRallyState(
                        Mission,
                        formation,
                        presence,
                        anchor,
                        _commanderRallySettings,
                        null);

                    _troopAbsorptionController.SyncFormation(
                        Mission,
                        formation,
                        presence,
                        rallyProbe.RallyPoint,
                        _commanderRallySettings,
                        _absorptionMissionTime,
                        plan,
                        _rowRankSlotAssigner,
                        _formationDataAdapter);

                    CommanderRallyState rallyFinal = _commanderRallyPlanner.BuildRallyState(
                        Mission,
                        formation,
                        presence,
                        anchor,
                        _commanderRallySettings,
                        _troopAbsorptionController);

                    if (_commanderConfig.EnableRallyAbsorptionDebug)
                    {
                        string label = formation.RepresentativeClass.ToString();
                        ModLogger.LogDebug(
                            $"{ModConstants.ModuleId}: Rally absorption: {label} {rallyFinal.TotalTroops} total, {rallyFinal.AbsorbableTroops} absorbable, {rallyFinal.AssignedTroops} assigned.");
                    }
                }
            }
            catch (System.Exception ex)
            {
                ModLogger.LogDebug($"{ModConstants.ModuleId}: Rally absorption scan skipped ({ex.Message})");
            }
        }

        private void MaybeScanDoctrineProfiles(float dt)
        {
            if (_doctrineScoreCalculator == null || _commanderAssignmentService == null || Mission?.PlayerTeam == null)
            {
                return;
            }

            float interval = System.Math.Max(0.1f, _commanderConfig.DoctrineScanIntervalSeconds);
            _doctrineScanAccum += dt;
            if (_doctrineScanAccum < interval)
            {
                return;
            }

            _doctrineScanAccum = 0f;

            if (!_commanderConfig.EnableDoctrineDebug)
            {
                return;
            }

            var sumByClass = new Dictionary<FormationClass, float>();
            var countByClass = new Dictionary<FormationClass, int>();
            DoctrineScoreSettings settings = DoctrineScoreSettings.FromConfig(_commanderConfig);

            try
            {
                foreach (Formation formation in Mission.PlayerTeam.FormationsIncludingEmpty)
                {
                    if (formation == null || formation.CountOfUnits <= 0)
                    {
                        continue;
                    }

                    CommanderPresenceResult presence = _commanderAssignmentService.DetectCommander(Mission, formation);
                    DoctrineScoreResult scored = _doctrineScoreCalculator.Compute(Mission, formation, presence, settings);
                    if (!scored.ComputationSucceeded || scored.Profile == null)
                    {
                        continue;
                    }

                    FormationClass cls = formation.RepresentativeClass;
                    if (!sumByClass.ContainsKey(cls))
                    {
                        sumByClass[cls] = 0f;
                        countByClass[cls] = 0;
                    }

                    sumByClass[cls] += scored.Profile.FormationDisciplineScore;
                    countByClass[cls]++;
                }
            }
            catch (System.Exception ex)
            {
                ModLogger.LogDebug($"{ModConstants.ModuleId}: Doctrine scan skipped ({ex.Message})");
                return;
            }

            if (sumByClass.Count == 0)
            {
                return;
            }

            var parts = new List<string>();
            foreach (KeyValuePair<FormationClass, float> kv in sumByClass)
            {
                int c = countByClass[kv.Key];
                float avg = c > 0 ? kv.Value / c : 0f;
                parts.Add($"{kv.Key} discipline {avg:F2}");
            }

            parts.Sort();
            ModLogger.LogDebug($"{ModConstants.ModuleId}: Doctrine scan: {string.Join(", ", parts)}.");
        }

        private static string FormatEligibilityTypes(List<AllowedFormationType> types)
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

        private void MaybeDebugCommandRouterKeys(float dt)
        {
            if (_commandRouter == null || !_commanderConfig.EnableCommandRouter || Mission?.PlayerTeam == null)
            {
                return;
            }

            _secondsSinceCommandValidationLog += dt;
            float logInterval = System.Math.Max(0.15f, _commanderConfig.CommandValidationDebugLogIntervalSeconds);
            IInputContext input = Input;
            if (input == null)
            {
                return;
            }

            try
            {
                Formation formation = TryPickFirstPlayerFormation();
                if (formation == null)
                {
                    return;
                }

                if (input.IsKeyReleased(InputKey.H))
                {
                    RunCommandRouterDebug(BuildBasicHoldIntent(formation), logInterval);
                    return;
                }

                if (input.IsKeyReleased(InputKey.C))
                {
                    RunCommandRouterDebug(BuildChargeIntent(formation), logInterval);
                    return;
                }

                if (input.IsKeyReleased(InputKey.M))
                {
                    CommandIntent advance = BuildAdvanceOrMoveIntent(formation);
                    if (advance == null)
                    {
                        if (_commanderConfig.EnableCommandValidationDebug && _secondsSinceCommandValidationLog >= logInterval)
                        {
                            ModLogger.LogDebug(
                                $"{ModConstants.ModuleId}: Cmd validate AdvanceOrMove: skipped (no placeholder target).");
                            _secondsSinceCommandValidationLog = 0f;
                        }

                        return;
                    }

                    RunCommandRouterDebug(advance, logInterval);
                }
            }
            catch (System.Exception ex)
            {
                ModLogger.LogDebug($"{ModConstants.ModuleId}: command router debug keys skipped ({ex.Message})");
            }
        }

        private Formation TryPickFirstPlayerFormation()
        {
            foreach (Formation formation in Mission.PlayerTeam.FormationsIncludingEmpty)
            {
                if (formation != null && formation.CountOfUnits > 0)
                {
                    return formation;
                }
            }

            return null;
        }

        private FormationDoctrineProfile ResolveDoctrineProfile(Formation formation, CommanderPresenceResult presence)
        {
            DoctrineScoreResult scored = _doctrineScoreCalculator.Compute(
                Mission,
                formation,
                presence,
                DoctrineScoreSettings.FromConfig(_commanderConfig));
            if (scored.ComputationSucceeded && scored.Profile != null)
            {
                return scored.Profile;
            }

            return new FormationDoctrineProfile(
                0f,
                0f,
                0f,
                0f,
                0f,
                0f,
                0f,
                0f,
                FormationCompositionProfile.Empty("doctrine unavailable"),
                "fallback",
                false);
        }

        private CommandContext BuildCommandContext(Formation formation, string sourceReason)
        {
            CommanderPresenceResult presence = _commanderAssignmentService.DetectCommander(Mission, formation);
            FormationDoctrineProfile doctrine = ResolveDoctrineProfile(formation, presence);
            FormationEligibilityResult elig = _formationEligibilityRules.Evaluate(formation, presence, doctrine);
            return new CommandContext(
                Mission,
                _commanderModeState.IsEnabled,
                presence,
                doctrine,
                elig,
                sourceReason);
        }

        private void RunCommandRouterDebug(CommandIntent intent, float logInterval)
        {
            CommandContext context = BuildCommandContext(intent.SourceFormation, intent.Source);
            CommandValidationResult validation = _commandRouter.Validate(intent, context);
            CommandExecutionDecision decision = _commandRouter.Decide(intent, context);
            if (_commanderConfig.EnableCommandValidationDebug && _secondsSinceCommandValidationLog >= logInterval)
            {
                ModLogger.LogDebug(
                    $"{ModConstants.ModuleId}: Cmd {intent.Type}: valid={validation.IsValid}, blocked={validation.IsBlocked}, {validation.Message}; execute={decision.ShouldExecute}, native={decision.RequiresNativeOrder}, cavSeq={decision.RequiresCavalrySequence}, prim={decision.NativePrimitive}, {decision.Reason}");
                _secondsSinceCommandValidationLog = 0f;
            }
        }

        private CommandIntent BuildBasicHoldIntent(Formation formation)
        {
            var intent = new CommandIntent
            {
                Type = CommandType.BasicHold,
                SourceFormation = formation,
                RequiresPosition = true,
                RequiresTargetFormation = false,
                RequiresCommander = false,
                Source = "debug-key-H"
            };
            FormationDataResult center = _formationDataAdapter.TryGetFormationCenter(formation);
            if (center.Success)
            {
                intent.TargetPosition = center.Vec3;
            }

            return intent;
        }

        private static CommandIntent BuildChargeIntent(Formation formation)
        {
            return new CommandIntent
            {
                Type = CommandType.Charge,
                SourceFormation = formation,
                RequiresPosition = false,
                RequiresTargetFormation = false,
                RequiresCommander = false,
                Source = "debug-key-C"
            };
        }

        private CommandIntent BuildAdvanceOrMoveIntent(Formation formation)
        {
            FormationDataResult center = _formationDataAdapter.TryGetFormationCenter(formation);
            if (!center.Success)
            {
                return null;
            }

            Vec3 target = center.Vec3;
            if (_cameraController.HasPose)
            {
                CommanderCameraPose pose = _cameraController.GetPose();
                float yaw = pose.Yaw;
                target = center.Vec3 + new Vec3((float)System.Math.Cos(yaw), (float)System.Math.Sin(yaw), 0f) * 12f;
            }

            return new CommandIntent
            {
                Type = CommandType.AdvanceOrMove,
                SourceFormation = formation,
                TargetPosition = target,
                RequiresPosition = true,
                RequiresTargetFormation = false,
                RequiresCommander = false,
                Source = "debug-key-M"
            };
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
            _doctrineScanAccum = 0f;
            _rallyScanAccum = 0f;
            _absorptionMissionTime = 0f;
            _secondsSinceCommandValidationLog = 0f;
            _troopAbsorptionController?.Clear();
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
