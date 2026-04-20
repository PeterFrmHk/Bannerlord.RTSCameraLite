using System;
using System.Collections.Generic;
using System.Diagnostics;
using Bannerlord.RTSCameraLite.Adapters;
using TaleWorlds.Core;
using Bannerlord.RTSCameraLite.Camera;
using Bannerlord.RTSCameraLite.Commander;
using Bannerlord.RTSCameraLite.Commands;
using Bannerlord.RTSCameraLite.Config;
using Bannerlord.RTSCameraLite.Core;
using Bannerlord.RTSCameraLite.Diagnostics;
using Bannerlord.RTSCameraLite.Doctrine;
using Bannerlord.RTSCameraLite.Equipment;
using Bannerlord.RTSCameraLite.Input;
using Bannerlord.RTSCameraLite.Tactical;
using Bannerlord.RTSCameraLite.UX;
using Bannerlord.RTSCameraLite.Performance;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.View.MissionViews;
using TwFormationSelectionService = Bannerlord.RTSCameraLite.Selection.FormationSelectionService;
using TwFormationSelectionState = Bannerlord.RTSCameraLite.Selection.FormationSelectionState;

namespace Bannerlord.RTSCameraLite.Mission
{
    /// <summary>
    /// Mission shell: commander mode, camera pose, input guards, presence/doctrine/eligibility scans (Slice 8+), commander anchor (Slice 9).
    /// </summary>
    public sealed class CommanderMissionView : MissionView
    {
        private readonly CommanderModeState _commanderModeState = new CommanderModeState();
        private readonly CommanderInputReader _commanderInput = new CommanderInputReader();
        private readonly CommandGestureParser _commandGestureParser = new CommandGestureParser();
        private readonly CameraBridge _cameraBridge = new CameraBridge();
        private readonly CommanderCameraController _cameraController = new CommanderCameraController();
        private readonly BackspaceConflictGuard _backspaceConflictGuard = new BackspaceConflictGuard();
        private readonly CommanderNativeInputGuard _nativeInputGuard = new CommanderNativeInputGuard();
        private readonly FormationDataAdapter _formationDataAdapter = new FormationDataAdapter();
        private CommanderAssignmentService _commanderAssignmentService;
        private FormationEligibilityRules _formationEligibilityRules;
        private DoctrineScoreCalculator _doctrineScoreCalculator;
        private CommanderConfig _commanderConfig = CommanderConfigDefaults.CreateDefault();
        private bool _loggedShellActive;
        private bool _lastLoggedEnabled;
        private bool _hasLoggedEnabledState;
        private bool _lifecycleCleanupDone;
        private bool _runtimeFaulted;
        private bool _runtimeFaultCleanupDone;
        private bool _loggedFirstInternalPose;
        private bool _loggedCameraBridgeNotAppliedWarning;
        private CommanderAnchorResolver _anchorResolver;
        private CommanderAnchorSettings _anchorSettings;
        private CommanderRallyPlanner _commanderRallyPlanner;
        private TroopAbsorptionController _troopAbsorptionController;
        private RowRankSlotAssigner _rowRankSlotAssigner;
        private CommanderRallySettings _commanderRallySettings;
        private float _absorptionMissionTime;
        private readonly Dictionary<Formation, CavalryChargeSequenceState> _cavalryDoctrineByFormation =
            new Dictionary<Formation, CavalryChargeSequenceState>();
        private readonly Dictionary<Formation, float> _cavalryWideLayoutLogTime = new Dictionary<Formation, float>();
        private CommandRouter _commandRouter;
        private NativeOrderPrimitiveExecutor _nativeOrderPrimitives;
        private readonly CavalrySequenceRegistry _cavalrySequenceRegistry = new CavalrySequenceRegistry();
        private CavalryNativeChargeOrchestrator _cavalryNativeChargeOrchestrator;
        private readonly Dictionary<Formation, NativeCavalrySequenceLogState> _cavalryNativeSeqTransitionLog =
            new Dictionary<Formation, NativeCavalrySequenceLogState>();
        private float _secondsSinceCommandValidationLog;
        private readonly CommanderDiagnosticsService _commanderDiagnosticsService = new CommanderDiagnosticsService();
        private FeedbackThrottle _feedbackThrottle;
        private TacticalFeedbackService _tacticalFeedback;
        private CommandMarkerService _commandMarkerService;
        private GroundTargetResolver _groundTargetResolver;
        private TwFormationSelectionState _twFormationSelectionState;
        private TwFormationSelectionService _twFormationSelectionService;
        private CommanderPerformanceBudget _perfBudget;
        private ThrottledUpdateGate _perfGate;
        private PerformanceDiagnosticsService _perfDiagnostics;
        private float _missionPerfClock;
        private float _markerTickAccumSeconds;

        private sealed class NativeCavalrySequenceLogState
        {
            public bool LoggedAdvance;

            public bool LoggedCharge;

            public bool LoggedLockReleased;

            public bool LoggedReformReady;

            public bool LoggedReassembled;
        }

        public CommanderModeState CommanderModeState => _commanderModeState;

        public override void OnBehaviorInitialize()
        {
            try
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
            if (!_commanderConfig.EnableMissionRuntimeHooks)
            {
                ModLogger.LogDebug($"{ModConstants.ModuleId}: mission runtime hooks false after config load; commander runtime stays dormant.");
                return;
            }

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
            _anchorResolver = new CommanderAnchorResolver(_formationDataAdapter);
            _anchorSettings = CommanderAnchorSettings.FromConfig(_commanderConfig);
            _commanderRallyPlanner = new CommanderRallyPlanner(_formationDataAdapter);
            _troopAbsorptionController = new TroopAbsorptionController();
            _rowRankSlotAssigner = new RowRankSlotAssigner(_formationDataAdapter);
            _commanderRallySettings = CommanderRallySettings.FromConfig(_commanderConfig);
            _perfBudget = CommanderPerformanceBudget.FromCommanderConfig(_commanderConfig);
            _perfGate = new ThrottledUpdateGate(_perfBudget);
            _perfDiagnostics = new PerformanceDiagnosticsService(_commanderConfig, _perfGate);
            _nativeOrderPrimitives = new NativeOrderPrimitiveExecutor(_commanderConfig);
            _commandRouter = new CommandRouter(_commanderConfig, _nativeOrderPrimitives);
            _cavalryNativeChargeOrchestrator = new CavalryNativeChargeOrchestrator(
                _commanderConfig,
                _formationDataAdapter,
                _nativeOrderPrimitives,
                _commanderAssignmentService,
                _cavalrySequenceRegistry);
            _secondsSinceCommandValidationLog = 0f;
            _feedbackThrottle = new FeedbackThrottle();
            _tacticalFeedback = new TacticalFeedbackService(_feedbackThrottle);
            _commandMarkerService = new CommandMarkerService(
                CommandMarkerSettings.FromConfig(_commanderConfig),
                _tacticalFeedback,
                () => Mission);
            _groundTargetResolver = new GroundTargetResolver();
            _twFormationSelectionState = new TwFormationSelectionState();
            _twFormationSelectionService = new TwFormationSelectionService();
            _missionPerfClock = 0f;
            _markerTickAccumSeconds = 0f;
            LogShellActiveOnce();
            LogEnabledTransition();
            }
            catch (Exception ex)
            {
                HandleRuntimeFault("OnBehaviorInitialize", ex);
            }
        }

        public override void OnMissionTick(float dt)
        {
            try
            {
                base.OnMissionTick(dt);

            if (_runtimeFaulted)
            {
                EnsureRuntimeFaultCleanup("runtime fault");
                return;
            }

            if (Mission == null)
            {
                return;
            }

            if (Mission.MissionEnded)
            {
                EnsureLifecycleCleanup("mission ended");
                return;
            }

            if (!CommanderMissionModeGate.IsSupportedMission(Mission))
            {
                return;
            }

            if (!_commanderConfig.EnableMissionRuntimeHooks)
            {
                return;
            }

            _absorptionMissionTime += dt;
            _missionPerfClock += dt;
            _perfDiagnostics?.TryWarnOverBudget(dt);
            if (_perfGate != null
                && _perfDiagnostics != null
                && _commanderConfig.EnablePerformanceDiagnostics
                && _perfGate.ShouldRun(UpdateBudgetCategory.FeedbackTick, _missionPerfClock))
            {
                _perfGate.MarkRun(UpdateBudgetCategory.FeedbackTick, _missionPerfClock, -1f);
                _perfDiagnostics.Tick(_missionPerfClock, dt);
            }

            MaybeProcessFormationSelectionInput();
            MaybeProcessGroundCommandPreviewInput();
            MaybeRunDoctrineAndCavalryScans(dt);
            MaybeScanFriendlyFormationCommanders(dt);
            MaybeScanRallyAbsorption(dt);
            MaybeTickNativeCavalryChargeSequences(dt);
            MaybeTickCommandMarkers(dt);
            MaybeDiagnostics(dt);

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
                MaybeDebugCommandRouterKeys(dt);
            }
            }
            catch (Exception ex)
            {
                HandleRuntimeFault("OnMissionTick", ex);
            }
        }

        public override void OnRemoveBehavior()
        {
            try
            {
                EnsureLifecycleCleanup("behavior removed");
                base.OnRemoveBehavior();
            }
            catch (Exception ex)
            {
                HandleRuntimeFault("OnRemoveBehavior", ex);
            }
        }

        private void MaybeRunDoctrineAndCavalryScans(float dt)
        {
            bool doctrineFeatureEnabled = _commanderConfig.EnableDoctrineDebug || _commanderConfig.EnableCavalryDoctrineDebug;
            if (!CanRunHeavyMissionFeature(doctrineFeatureEnabled))
            {
                return;
            }

            if (_perfGate == null || _doctrineScoreCalculator == null || Mission?.PlayerTeam == null)
            {
                return;
            }

            if (!_perfGate.ShouldRun(UpdateBudgetCategory.DoctrineScan, _missionPerfClock))
            {
                return;
            }

            Stopwatch sw = Stopwatch.StartNew();
            try
            {
                MaybeScanDoctrineProfiles(dt);
                MaybeEvaluateCavalryDoctrine(dt);
            }
            finally
            {
                _perfGate.MarkRun(UpdateBudgetCategory.DoctrineScan, _missionPerfClock, (float)sw.Elapsed.TotalSeconds);
            }
        }

        private void MaybeScanFriendlyFormationCommanders(float dt)
        {
            bool commanderScanFeatureEnabled = _commanderConfig.EnableEligibilityDebug
                                               || (_commanderModeState.IsEnabled && _commanderConfig.EnableCommanderAnchorDebug);
            if (!CanRunHeavyMissionFeature(commanderScanFeatureEnabled))
            {
                return;
            }

            if (_perfGate == null || _commanderAssignmentService == null || Mission?.PlayerTeam == null)
            {
                return;
            }

            if (!_perfGate.ShouldRun(UpdateBudgetCategory.CommanderScan, _missionPerfClock))
            {
                return;
            }

            Stopwatch sw = Stopwatch.StartNew();
            bool runEligibilityDebug = _commanderConfig.EnableEligibilityDebug
                                       && _formationEligibilityRules != null
                                       && _doctrineScoreCalculator != null
                                       && _perfGate.ShouldRun(UpdateBudgetCategory.EligibilityScan, _missionPerfClock);
            if (runEligibilityDebug)
            {
                _perfGate.MarkRun(UpdateBudgetCategory.EligibilityScan, _missionPerfClock, -1f);
            }

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

                    if (runEligibilityDebug)
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

                if (_commanderModeState.IsEnabled)
                {
                    RunCommanderAnchorScanBody();
                }
            }
            catch (System.Exception ex)
            {
                ModLogger.LogDebug($"{ModConstants.ModuleId}: Commander scan skipped ({ex.Message})");
            }
            finally
            {
                _perfGate.MarkRun(UpdateBudgetCategory.CommanderScan, _missionPerfClock, (float)sw.Elapsed.TotalSeconds);
            }

            ModLogger.LogDebug($"{ModConstants.ModuleId}: Commander scan: {commanded}/{total} formations commanded");
        }

        /// <summary>
        /// Slice 12 — rally/absorption and slot planning only (no native orders, no forced movement).
        /// </summary>
        private void MaybeScanRallyAbsorption(float dt)
        {
            if (!CanRunHeavyMissionFeature(_commanderConfig.EnableRallyAbsorptionDebug))
            {
                return;
            }

            if (_perfGate == null
                || _commanderRallyPlanner == null
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

            if (!_perfGate.ShouldRun(UpdateBudgetCategory.RallyAbsorptionScan, _missionPerfClock))
            {
                return;
            }

            Stopwatch sw = Stopwatch.StartNew();
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
                        presence,
                        _commanderConfig);

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
            finally
            {
                _perfGate?.MarkRun(UpdateBudgetCategory.RallyAbsorptionScan, _missionPerfClock, (float)sw.Elapsed.TotalSeconds);
            }
        }

        /// <summary>
        /// Slice 13 — cavalry spacing / lock-release / reform doctrine (planning and logs only; no native orders).
        /// </summary>
        private void MaybeEvaluateCavalryDoctrine(float dt)
        {
            if (_commanderAssignmentService == null
                || _doctrineScoreCalculator == null
                || _formationEligibilityRules == null
                || _commanderRallyPlanner == null
                || _commanderRallySettings == null
                || _anchorResolver == null
                || Mission?.PlayerTeam == null
                || _perfBudget == null)
            {
                return;
            }

            float interval = System.Math.Max(0.1f, _perfBudget.DoctrineScanIntervalSeconds);
            DoctrineScoreSettings doctrineSettings = DoctrineScoreSettings.FromConfig(_commanderConfig);
            var toRemove = new List<Formation>();

            try
            {
                foreach (Formation formation in Mission.PlayerTeam.FormationsIncludingEmpty)
                {
                    if (formation == null || formation.CountOfUnits <= 0)
                    {
                        if (formation != null)
                        {
                            toRemove.Add(formation);
                        }

                        continue;
                    }

                    FormationCompositionProfile comp = FormationCompositionAnalyzer.Analyze(_formationDataAdapter, formation);
                    if (!CavalrySpacingRules.IsCavalryHeavyFormation(comp))
                    {
                        if (_cavalryDoctrineByFormation.ContainsKey(formation))
                        {
                            toRemove.Add(formation);
                        }

                        continue;
                    }

                    if (_commanderConfig.EnableNativeCavalryChargeSequence
                        && _cavalrySequenceRegistry.TryGetSequence(formation, out CavalryChargeSequenceState nativeSeq)
                        && nativeSeq != null
                        && !nativeSeq.Aborted
                        && nativeSeq.CurrentState != CavalryChargeState.Reassembled)
                    {
                        continue;
                    }

                    CommanderPresenceResult presence = _commanderAssignmentService.DetectCommander(Mission, formation);
                    CommanderAnchorState anchor = _anchorResolver.ResolveAnchor(Mission, formation, presence, _anchorSettings);
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
                    RowRankSpacingPlan plan = FormationLayoutPlanner.Build(
                        formation,
                        doctrineRes.Profile,
                        elig,
                        comp,
                        presence,
                        _commanderConfig);

                    CommanderRallyState rallyFinal = _commanderRallyPlanner.BuildRallyState(
                        Mission,
                        formation,
                        presence,
                        anchor,
                        _commanderRallySettings,
                        _troopAbsorptionController);

                    _cavalryDoctrineByFormation.TryGetValue(formation, out CavalryChargeSequenceState prev);
                    CavalryChargeSequenceState next = CavalryDoctrineRules.Evaluate(
                        Mission,
                        formation,
                        comp,
                        doctrineRes.Profile,
                        presence,
                        rallyFinal,
                        _commanderConfig,
                        _formationDataAdapter,
                        plan,
                        prev,
                        interval);

                    _cavalryDoctrineByFormation[formation] = next;

                    if (!_commanderConfig.EnableCavalryDoctrineDebug)
                    {
                        continue;
                    }

                    string label = formation.RepresentativeClass.ToString();
                    if (prev == null || next.CurrentState != prev.CurrentState)
                    {
                        ModLogger.LogDebug(
                            $"{ModConstants.ModuleId}: Cavalry doctrine {label}: state={next.CurrentState} ({next.Reason})");
                    }

                    if (plan.IsMountedLayout
                        && (!_cavalryWideLayoutLogTime.TryGetValue(formation, out float lastWide)
                            || _absorptionMissionTime - lastWide >= 10f))
                    {
                        _cavalryWideLayoutLogTime[formation] = _absorptionMissionTime;
                        ModLogger.LogDebug($"{ModConstants.ModuleId}: Cavalry layout: wide spacing. ({label})");
                    }

                    if (next.PositionLockReleased && (prev == null || !prev.PositionLockReleased))
                    {
                        ModLogger.LogDebug($"{ModConstants.ModuleId}: Cavalry close contact: position lock released. ({label})");
                    }

                    if (next.ReformAllowed && (prev == null || !prev.ReformAllowed))
                    {
                        ModLogger.LogDebug($"{ModConstants.ModuleId}: Cavalry reform ready. ({label})");
                    }
                }

                for (int i = 0; i < toRemove.Count; i++)
                {
                    Formation f = toRemove[i];
                    _cavalryDoctrineByFormation.Remove(f);
                    _cavalryWideLayoutLogTime.Remove(f);
                }
            }
            catch (Exception ex)
            {
                ModLogger.LogDebug($"{ModConstants.ModuleId}: Cavalry doctrine scan skipped ({ex.Message})");
            }
        }

        /// <summary>
        /// Slice 16 — throttled tick for native cavalry charge orchestration (no per-frame heavy scans).
        /// </summary>
        private void MaybeTickNativeCavalryChargeSequences(float dt)
        {
            if (!CanRunHeavyMissionFeature(_commanderConfig.EnableNativeCavalryChargeSequence))
            {
                return;
            }

            if (!_commanderConfig.EnableNativeCavalryChargeSequence
                || _cavalryNativeChargeOrchestrator == null
                || Mission?.PlayerTeam == null
                || _perfGate == null
                || _perfBudget == null)
            {
                return;
            }

            if (!_perfGate.ShouldRun(UpdateBudgetCategory.CavalrySequenceTick, _missionPerfClock))
            {
                return;
            }

            Stopwatch sw = Stopwatch.StartNew();
            float tickSeconds = System.Math.Max(0.01f, _perfBudget.CavalrySequenceIntervalSeconds);
            try
            {
                _cavalrySequenceRegistry.CleanupInvalidSequences(Mission);
                var finished = new List<Formation>();
                _cavalrySequenceRegistry.ForEachActive(
                    (formation, state) =>
                    {
                        if (formation == null || state == null)
                        {
                            return;
                        }

                        bool prevForward = state.NativeForwardIssued;
                        bool prevCharge = state.NativeChargeIssued;
                        bool prevLock = state.PositionLockReleased;
                        bool prevReformAllowed = state.ReformAllowed;
                        CavalryChargeState prevState = state.CurrentState;

                        CavalrySequenceTickResult tick = _cavalryNativeChargeOrchestrator.TickSequence(
                            state,
                            tickSeconds);

                        if (_commanderConfig.EnableCavalrySequenceDebug)
                        {
                            if (!_cavalryNativeSeqTransitionLog.TryGetValue(formation, out NativeCavalrySequenceLogState logState))
                            {
                                logState = new NativeCavalrySequenceLogState();
                                _cavalryNativeSeqTransitionLog[formation] = logState;
                            }

                            if (state.NativeForwardIssued && !prevForward && !logState.LoggedAdvance)
                            {
                                logState.LoggedAdvance = true;
                                ModLogger.LogDebug($"{ModConstants.ModuleId}: Cavalry advancing.");
                            }

                            if (state.NativeChargeIssued && !prevCharge && !logState.LoggedCharge)
                            {
                                logState.LoggedCharge = true;
                                ModLogger.LogDebug($"{ModConstants.ModuleId}: Cavalry charge issued.");
                            }

                            if (state.PositionLockReleased && !prevLock && !logState.LoggedLockReleased)
                            {
                                logState.LoggedLockReleased = true;
                                ModLogger.LogDebug($"{ModConstants.ModuleId}: Cavalry lock released.");
                            }

                            if (state.ReformAllowed && !prevReformAllowed && !logState.LoggedReformReady)
                            {
                                logState.LoggedReformReady = true;
                                ModLogger.LogDebug($"{ModConstants.ModuleId}: Cavalry reform ready.");
                                MaybeShowReformPointMarker(formation);
                            }

                            if (state.CurrentState == CavalryChargeState.Reassembled
                                && prevState != CavalryChargeState.Reassembled
                                && !logState.LoggedReassembled)
                            {
                                logState.LoggedReassembled = true;
                                ModLogger.LogDebug($"{ModConstants.ModuleId}: Cavalry reassembled.");
                            }
                        }

                        if (tick.Completed || tick.Aborted)
                        {
                            finished.Add(formation);
                        }
                    });

                for (int i = 0; i < finished.Count; i++)
                {
                    Formation f = finished[i];
                    _cavalrySequenceRegistry.StopSequence(f);
                    _cavalryNativeSeqTransitionLog.Remove(f);
                }
            }
            catch (Exception ex)
            {
                ModLogger.LogDebug($"{ModConstants.ModuleId}: Native cavalry sequence tick skipped ({ex.Message})");
            }
            finally
            {
                _perfGate.MarkRun(UpdateBudgetCategory.CavalrySequenceTick, _missionPerfClock, (float)sw.Elapsed.TotalSeconds);
            }
        }

        private void MaybeTickCommandMarkers(float dt)
        {
            if (!CanRunHeavyMissionFeature(_commanderConfig.EnableCommandMarkers))
            {
                return;
            }

            try
            {
                if (_commandMarkerService != null && _perfGate != null)
                {
                    _markerTickAccumSeconds += dt;
                    if (_perfGate.ShouldRun(UpdateBudgetCategory.MarkerTick, _missionPerfClock))
                    {
                        _perfGate.MarkRun(UpdateBudgetCategory.MarkerTick, _missionPerfClock, -1f);
                        _commandMarkerService.Tick(_markerTickAccumSeconds);
                        _markerTickAccumSeconds = 0f;
                    }
                }

                if (_commandMarkerService == null
                    || !_commanderConfig.EnableCommandMarkers
                    || Mission == null
                    || Mission.MissionEnded
                    || !_commanderModeState.IsEnabled
                    || !_cameraController.HasPose
                    || _perfGate == null
                    || _groundTargetResolver == null)
                {
                    return;
                }

                if (!_perfGate.ShouldRun(UpdateBudgetCategory.Targeting, _missionPerfClock))
                {
                    return;
                }

                Stopwatch sw = Stopwatch.StartNew();
                try
                {
                    CommanderCameraPose pose = _cameraController.GetPose();
                    GroundTargetResult ground = _groundTargetResolver.TryResolveFromCamera(
                        Mission,
                        pose.Position,
                        pose.Yaw,
                        pose.Pitch);
                    if (ground.Success)
                    {
                        _commandMarkerService.AddGroundTargetMarker(ground);
                    }
                }
                finally
                {
                    _perfGate.MarkRun(UpdateBudgetCategory.Targeting, _missionPerfClock, (float)sw.Elapsed.TotalSeconds);
                }
            }
            catch (Exception ex)
            {
                ModLogger.LogDebug($"{ModConstants.ModuleId}: command markers tick skipped ({ex.Message})");
            }
        }

        private void MaybeShowReformPointMarker(Formation formation)
        {
            try
            {
                if (_commandMarkerService == null
                    || !_commanderConfig.EnableCommandMarkers
                    || formation == null
                    || _formationDataAdapter == null)
                {
                    return;
                }

                FormationDataResult c = _formationDataAdapter.TryGetFormationCenter(formation);
                if (c.Success)
                {
                    _commandMarkerService.AddReformMarker(c.Vec3);
                }
            }
            catch (Exception ex)
            {
                ModLogger.LogDebug($"{ModConstants.ModuleId}: reform marker skipped ({ex.Message})");
            }
        }

        private void MaybeScanDoctrineProfiles(float dt)
        {
            if (_doctrineScoreCalculator == null || _commanderAssignmentService == null || Mission?.PlayerTeam == null)
            {
                return;
            }

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

        private void RunCommanderAnchorScanBody()
        {
            if (_anchorResolver == null || _commanderAssignmentService == null || Mission == null)
            {
                return;
            }

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

        private void MaybeProcessFormationSelectionInput()
        {
            if (_runtimeFaulted || Mission == null || Mission.MissionEnded)
            {
                return;
            }

            if (!_commanderConfig.EnableMissionRuntimeHooks || !_commanderConfig.EnableFormationSelection)
            {
                return;
            }

            IInputContext input = Input;
            if (input == null)
            {
                return;
            }

            int slot;
            if (!_commanderInput.TryConsumeFormationSelectionSlot(input, out slot))
            {
                return;
            }

            if (!HasPlayerTeamSafe())
            {
                ModLogger.LogDebug($"{ModConstants.ModuleId}: TW-1 formation selection skipped (no player team).");
                return;
            }

            if (_twFormationSelectionState == null)
            {
                _twFormationSelectionState = new TwFormationSelectionState();
            }

            if (_twFormationSelectionService == null)
            {
                _twFormationSelectionService = new TwFormationSelectionService();
            }

            var result = _twFormationSelectionService.TrySelectNumberKeySlot(
                Mission,
                _commanderConfig.EnableMissionRuntimeHooks,
                _commanderConfig.EnableFormationSelection,
                slot,
                _twFormationSelectionState);

            if (result == null || !result.Handled)
            {
                return;
            }

            bool allowUi = _commanderModeState.IsEnabled && _tacticalFeedback != null;
            if (allowUi)
            {
                _tacticalFeedback.ShowFormationSelectionFeedback(result.Message, result.Slot, allowUi: true);
            }
            else
            {
                ModLogger.LogDebug($"{ModConstants.ModuleId}: {result.Message}");
            }
        }

        private bool HasPlayerTeamSafe()
        {
            try
            {
                return Mission?.PlayerTeam != null;
            }
            catch
            {
                return false;
            }
        }

        private void MaybeProcessGroundCommandPreviewInput()
        {
            if (_runtimeFaulted || Mission == null || Mission.MissionEnded)
            {
                return;
            }

            if (!_commanderConfig.EnableMissionRuntimeHooks
                || !_commanderConfig.EnableFormationSelection
                || !_commanderConfig.EnableGroundCommandPreview
                || !_commanderModeState.IsEnabled)
            {
                return;
            }

            IInputContext input = Input;
            if (input == null)
            {
                return;
            }

            GroundCommandPreviewRequest request = _commandGestureParser.TryReadGroundCommandPreview(
                input,
                _commanderConfig.EnableMissionRuntimeHooks,
                _commanderConfig.EnableFormationSelection,
                _commanderConfig.EnableGroundCommandPreview,
                _commanderModeState.IsEnabled);

            if (!request.Requested)
            {
                return;
            }

            if (_twFormationSelectionState == null
                || !_twFormationSelectionState.TryGetPrimarySelectedFormation(out Formation selectedFormation))
            {
                ShowGroundPreviewWarning("No formation selected");
                return;
            }

            if (_groundTargetResolver == null || !_cameraController.HasPose)
            {
                ShowGroundPreviewWarning("No valid ground target");
                return;
            }

            GroundTargetResult ground;
            try
            {
                CommanderCameraPose pose = _cameraController.GetPose();
                ground = _groundTargetResolver.TryResolveFromCamera(
                    Mission,
                    pose.Position,
                    pose.Yaw,
                    pose.Pitch);
            }
            catch
            {
                ShowGroundPreviewWarning("No valid ground target");
                return;
            }

            if (!ground.Success)
            {
                ShowGroundPreviewWarning("No valid ground target");
                return;
            }

            CommandIntent preview = BuildGroundPreviewIntent(selectedFormation, ground.Position);
            if (preview == null)
            {
                ShowGroundPreviewWarning("No formation selected");
                return;
            }

            MaybeShowGroundPreview(preview, selectedFormation);
            if (request.ExecuteRequested)
            {
                CommandIntent executeIntent = BuildGroundMoveExecutionIntent(selectedFormation, ground.Position);
                TryExecuteGroundMove(executeIntent, selectedFormation);
            }
        }

        private CommandIntent BuildGroundPreviewIntent(Formation formation, Vec3 target)
        {
            if (formation == null)
            {
                return null;
            }

            return new CommandIntent
            {
                Type = CommandType.AdvanceOrMove,
                SourceFormation = formation,
                TargetPosition = target,
                RequiresPosition = true,
                RequiresTargetFormation = false,
                RequiresCommander = false,
                Source = "tw-ground-preview"
            };
        }

        private CommandIntent BuildGroundMoveExecutionIntent(Formation formation, Vec3 target)
        {
            if (formation == null)
            {
                return null;
            }

            return new CommandIntent
            {
                Type = CommandType.AdvanceOrMove,
                SourceFormation = formation,
                TargetPosition = target,
                RequiresPosition = true,
                RequiresTargetFormation = false,
                RequiresCommander = false,
                Source = "tw-ground-execute"
            };
        }

        private void TryExecuteGroundMove(CommandIntent intent, Formation formation)
        {
            string label = SafeFormationLabel(formation);
            try
            {
                if (intent == null || intent.SourceFormation == null || !intent.TargetPosition.HasValue)
                {
                    ShowGroundMoveExecutionFeedback($"Move order failed: invalid move intent");
                    return;
                }

                if (!CanExecuteGroundMove(out string gateReason))
                {
                    ShowGroundMoveExecutionFeedback($"Move order blocked: {gateReason}");
                    return;
                }

                if (_commandRouter == null)
                {
                    ShowGroundMoveExecutionFeedback("Move order blocked: command router unavailable");
                    return;
                }

                CommandContext context = BuildCommandContext(intent.SourceFormation, intent.Source);
                CommandValidationResult validation = _commandRouter.Validate(intent, context);
                if (validation == null || !validation.IsValid || validation.IsBlocked)
                {
                    string reason = validation == null ? "validation unavailable" : validation.Message;
                    ShowGroundMoveExecutionFeedback($"Move order blocked: {reason}");
                    return;
                }

                var nativeContext = new NativeOrderExecutionContext(
                    Mission,
                    intent.SourceFormation,
                    targetFormation: null,
                    targetPosition: intent.TargetPosition,
                    targetDirection: null,
                    sourceReason: intent.Source,
                    eligibility: context?.Eligibility,
                    followTargetAgent: null);

                NativeOrderResult result = _nativeOrderPrimitives.ExecuteAdvanceOrMove(nativeContext);
                if (result == null)
                {
                    ShowGroundMoveExecutionFeedback("Move order failed: native executor returned no result");
                    return;
                }

                if (result.Executed)
                {
                    ShowGroundMoveExecutionFeedback($"Move order issued: {label}");
                    return;
                }

                if (result.Blocked)
                {
                    ShowGroundMoveExecutionFeedback($"Move order blocked: {result.Message}");
                    return;
                }

                ShowGroundMoveExecutionFeedback($"Move order failed: {result.Message}");
            }
            catch (Exception ex)
            {
                ShowGroundMoveExecutionFeedback($"Move order failed: {ex.Message}");
            }
        }

        private bool CanExecuteGroundMove(out string reason)
        {
            return GroundMoveExecutionGate.CanExecute(
                _commanderConfig,
                nativeExecutorAvailable: _nativeOrderPrimitives != null,
                out reason);
        }

        private void MaybeShowGroundPreview(CommandIntent preview, Formation formation)
        {
            if (preview == null || !preview.TargetPosition.HasValue)
            {
                return;
            }

            string label = SafeFormationLabel(formation);
            try
            {
                if (_commanderConfig.EnableCommandMarkers && _commandMarkerService != null)
                {
                    _commandMarkerService.AddMarker(
                        CommandMarkerType.MoveTarget,
                        preview.TargetPosition.Value,
                        "Move preview",
                        "tw-ground-preview");
                    return;
                }
            }
            catch (Exception ex)
            {
                ModLogger.LogDebug($"{ModConstants.ModuleId}: TW-2 move preview marker skipped ({ex.Message})");
            }

            if (_tacticalFeedback != null)
            {
                _tacticalFeedback.ShowGroundCommandPreview(preview, label, allowUi: _commanderModeState.IsEnabled);
            }
            else
            {
                Vec3 p = preview.TargetPosition.Value;
                ModLogger.LogDebug($"{ModConstants.ModuleId}: Preview move: {label} -> ({p.x:0.#},{p.y:0.#},{p.z:0.#})");
            }
        }

        private void ShowGroundPreviewWarning(string message)
        {
            if (_tacticalFeedback != null)
            {
                _tacticalFeedback.ShowGroundCommandPreviewWarning(message, allowUi: _commanderModeState.IsEnabled);
            }
            else
            {
                ModLogger.LogDebug($"{ModConstants.ModuleId}: {message}");
            }
        }

        private void ShowGroundMoveExecutionFeedback(string message)
        {
            if (_tacticalFeedback != null)
            {
                _tacticalFeedback.ShowGroundMoveExecutionResult(message, allowUi: _commanderModeState.IsEnabled);
            }
            else
            {
                ModLogger.LogDebug($"{ModConstants.ModuleId}: {message}");
            }
        }

        private static string SafeFormationLabel(Formation formation)
        {
            if (formation == null)
            {
                return "Formation";
            }

            try
            {
                return formation.RepresentativeClass.ToString();
            }
            catch
            {
                return "Formation";
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
            if (!CanRunHeavyMissionFeature(
                    _commanderConfig.EnableCommandRouter && _commanderConfig.EnableCommandValidationDebug))
            {
                return;
            }

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

                if (input.IsKeyReleased(InputKey.N))
                {
                    CommandIntent cavSeq = BuildNativeCavalryChargeSequenceIntent(formation);
                    if (cavSeq == null)
                    {
                        if (_commanderConfig.EnableCavalrySequenceDebug && _secondsSinceCommandValidationLog >= logInterval)
                        {
                            ModLogger.LogDebug(
                                $"{ModConstants.ModuleId}: Cmd validate NativeCavalryChargeSequence: skipped (no enemy target).");
                            _secondsSinceCommandValidationLog = 0f;
                        }

                        return;
                    }

                    CommandContext ctxN = BuildCommandContext(formation, "debug-key-N");
                    if (_commandRouter.TryStartNativeCavalryChargeSequence(cavSeq, ctxN, _cavalryNativeChargeOrchestrator, out string startMsg))
                    {
                        _cavalryNativeSeqTransitionLog[formation] = new NativeCavalrySequenceLogState();
                        MaybeShowChargeTargetMarker(cavSeq);
                    }
                    else if (_commanderConfig.EnableCavalrySequenceDebug && _secondsSinceCommandValidationLog >= logInterval)
                    {
                        ModLogger.LogDebug(
                            $"{ModConstants.ModuleId}: Cmd NativeCavalryChargeSequence not started: {startMsg}");
                        _secondsSinceCommandValidationLog = 0f;
                    }

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
            if (!CanRunHeavyMissionFeature(
                    _commanderConfig.EnableCommandRouter && _commanderConfig.EnableCommandValidationDebug))
            {
                return null;
            }

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
            MaybeShowMoveTargetMarkerForValidatedIntent(intent, validation);
            if (_commanderConfig.EnableCommandValidationDebug && _secondsSinceCommandValidationLog >= logInterval)
            {
                ModLogger.LogDebug(
                    $"{ModConstants.ModuleId}: Cmd {intent.Type}: valid={validation.IsValid}, blocked={validation.IsBlocked}, {validation.Message}; execute={decision.ShouldExecute}, native={decision.RequiresNativeOrder}, cavSeq={decision.RequiresCavalrySequence}, prim={decision.NativePrimitive}, {decision.Reason}");
                _secondsSinceCommandValidationLog = 0f;
            }
        }

        private void MaybeShowMoveTargetMarkerForValidatedIntent(CommandIntent intent, CommandValidationResult validation)
        {
            try
            {
                if (_commandMarkerService == null || !_commanderConfig.EnableCommandMarkers)
                {
                    return;
                }

                if (intent == null || !validation.IsValid || validation.IsBlocked)
                {
                    return;
                }

                if (intent.Type != CommandType.AdvanceOrMove || !intent.TargetPosition.HasValue)
                {
                    return;
                }

                Vec3 p = intent.TargetPosition.Value;
                _commandMarkerService.AddMarker(
                    CommandMarkerType.MoveTarget,
                    p,
                    "Move",
                    intent.Source ?? "command-router-debug");
            }
            catch (Exception ex)
            {
                ModLogger.LogDebug($"{ModConstants.ModuleId}: move target marker skipped ({ex.Message})");
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

        private void MaybeShowChargeTargetMarker(CommandIntent cavSeq)
        {
            try
            {
                if (_commandMarkerService == null
                    || !_commanderConfig.EnableCommandMarkers
                    || cavSeq?.TargetFormation == null
                    || _formationDataAdapter == null)
                {
                    return;
                }

                FormationDataResult c = _formationDataAdapter.TryGetFormationCenter(cavSeq.TargetFormation);
                if (!c.Success)
                {
                    return;
                }

                FormationTargetResult t = FormationTargetResult.At(c.Vec3, cavSeq.TargetFormation);
                _commandMarkerService.AddChargeTargetMarker(t);
            }
            catch (Exception ex)
            {
                ModLogger.LogDebug($"{ModConstants.ModuleId}: charge target marker skipped ({ex.Message})");
            }
        }

        private CommandIntent BuildNativeCavalryChargeSequenceIntent(Formation formation)
        {
            if (Mission == null || formation == null || _formationDataAdapter == null)
            {
                return null;
            }

            if (!CavalryTargetTracker.TryGetNearestEnemyFormation(Mission, formation, _formationDataAdapter, out Formation enemy))
            {
                return null;
            }

            return new CommandIntent
            {
                Type = CommandType.NativeCavalryChargeSequence,
                SourceFormation = formation,
                TargetFormation = enemy,
                RequiresPosition = false,
                RequiresTargetFormation = false,
                RequiresCommander = true,
                Source = "debug-key-N"
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

        /// <summary>Slice 20 — throttled text diagnostics (F9 toggle); no overlay.</summary>
        private void MaybeDiagnostics(float dt)
        {
            if (!CanRunHeavyMissionFeature(_commanderConfig.EnableDiagnostics))
            {
                return;
            }

            if (_tacticalFeedback == null)
            {
                return;
            }

            DiagnosticsSettings ds = DiagnosticsSettings.FromCommanderConfig(_commanderConfig);
            if (!ds.EnableDiagnostics)
            {
                return;
            }

            if (_commanderInput.TryConsumeDiagnosticsToggle(Input))
            {
                _commanderDiagnosticsService.ToggleDiagnostics();
                bool on = _commanderDiagnosticsService.IsVisible;
                _tacticalFeedback.ShowDiagnosticsSummary(on ? "ON" : "OFF", 0.2, true);
            }

            _commanderDiagnosticsService.Tick(dt);

            bool gateMode = !ds.ShowDiagnosticsInCommanderModeOnly || _commanderModeState.IsEnabled;
            if (!_commanderDiagnosticsService.IsVisible || !gateMode)
            {
                return;
            }

            if (!_commanderDiagnosticsService.ShouldPublishRefresh(dt, ds.DiagnosticsRefreshIntervalSeconds))
            {
                return;
            }

            if (_perfGate == null || !_perfGate.ShouldRun(UpdateBudgetCategory.DiagnosticsTick, _missionPerfClock))
            {
                return;
            }

            Stopwatch diagSw = Stopwatch.StartNew();
            try
            {
                var snaps = new List<FormationDiagnosticsSnapshot>();
                if (Mission?.PlayerTeam != null)
                {
                    foreach (Formation f in Mission.PlayerTeam.FormationsIncludingEmpty)
                    {
                        FormationDiagnosticsSnapshot s = TryBuildDiagnosticsSnapshot(f);
                        if (s != null)
                        {
                            snaps.Add(s);
                        }
                    }
                }

                if (snaps.Count == 0)
                {
                    return;
                }

                string block = FormationDiagnosticsFormatter.FormatMultiFormationBlock(snaps, ds);
                if (string.IsNullOrEmpty(block))
                {
                    return;
                }

                if (_commanderConfig.EnablePerformanceDiagnostics && _perfDiagnostics != null)
                {
                    string perf = _perfDiagnostics.BuildScanRateSummary();
                    if (!string.IsNullOrEmpty(perf))
                    {
                        block = block + "\n— perf — " + perf;
                    }
                }

                double cd = System.Math.Max(0.45, ds.DiagnosticsRefreshIntervalSeconds * 0.92);
                _tacticalFeedback.ShowDiagnosticsSummary(block, cd, false);
            }
            catch (Exception ex)
            {
                ModLogger.LogDebug($"{ModConstants.ModuleId}: diagnostics skipped ({ex.Message})");
            }
            finally
            {
                _perfGate?.MarkRun(UpdateBudgetCategory.DiagnosticsTick, _missionPerfClock, (float)diagSw.Elapsed.TotalSeconds);
            }
        }

        private FormationDiagnosticsSnapshot TryBuildDiagnosticsSnapshot(Formation formation)
        {
            if (_commanderAssignmentService == null
                || _anchorResolver == null
                || _formationEligibilityRules == null
                || _commanderRallyPlanner == null
                || formation == null
                || Mission == null)
            {
                return null;
            }

            try
            {
                if (formation.CountOfUnits <= 0)
                {
                    return null;
                }

                CommanderPresenceResult presence = _commanderAssignmentService.DetectCommander(Mission, formation);
                CommanderAnchorState anchor = _anchorResolver.ResolveAnchor(Mission, formation, presence, _anchorSettings);
                FormationDoctrineProfile doctrine = ResolveDoctrineProfile(formation, presence);
                FormationEligibilityResult elig = _formationEligibilityRules.Evaluate(formation, presence, doctrine);
                CommanderRallyState rally = _commanderRallyPlanner.BuildRallyState(
                    Mission,
                    formation,
                    presence,
                    anchor,
                    _commanderRallySettings,
                    _troopAbsorptionController);
                _cavalryDoctrineByFormation.TryGetValue(formation, out CavalryChargeSequenceState docCav);
                _cavalrySequenceRegistry.TryGetSequence(formation, out CavalryChargeSequenceState natCav);
                string tgt = BuildDiagnosticsTargetSummary(formation);
                return _commanderDiagnosticsService.BuildSnapshot(
                    formation,
                    presence,
                    anchor,
                    doctrine,
                    elig,
                    rally,
                    docCav,
                    natCav,
                    tgt,
                    _commanderConfig);
            }
            catch
            {
                return null;
            }
        }

        private string BuildDiagnosticsTargetSummary(Formation formation)
        {
            try
            {
                if (Mission == null || formation == null)
                {
                    return "—";
                }

                if (CavalryTargetTracker.TryGetNearestEnemyFormation(Mission, formation, _formationDataAdapter, out Formation enemy)
                    && enemy != null)
                {
                    try
                    {
                        return "En:" + enemy.RepresentativeClass;
                    }
                    catch
                    {
                        return "En:?";
                    }
                }

                if (_commanderModeState.IsEnabled && _cameraController != null && _cameraController.HasPose && _groundTargetResolver != null)
                {
                    CommanderCameraPose p = _cameraController.GetPose();
                    GroundTargetResult g = _groundTargetResolver.TryResolveFromCamera(Mission, p.Position, p.Yaw, p.Pitch);
                    return g.Success ? "GrndOK" : "Grnd—";
                }

                return "—";
            }
            catch
            {
                return "—";
            }
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
            _absorptionMissionTime = 0f;
            _missionPerfClock = 0f;
            _markerTickAccumSeconds = 0f;
            _perfGate?.ResetAll();
            _perfDiagnostics?.Reset();
            _cavalryDoctrineByFormation.Clear();
            _cavalryWideLayoutLogTime.Clear();
            _cavalrySequenceRegistry.Clear();
            _cavalryNativeSeqTransitionLog.Clear();
            _twFormationSelectionState?.Clear();
            _commandMarkerService?.Cleanup();
            _tacticalFeedback?.ResetSession();
            _secondsSinceCommandValidationLog = 0f;
            _troopAbsorptionController?.Clear();
            _cameraController.Reset();
            _commanderDiagnosticsService?.Cleanup();
            _feedbackThrottle?.ClearKey("diagnostics-summary");
        }

        private bool CanRunHeavyMissionFeature(bool featureEnabled)
        {
            if (!featureEnabled)
            {
                return false;
            }

            if (_runtimeFaulted || !_commanderConfig.EnableMissionRuntimeHooks)
            {
                return false;
            }

            return Mission?.PlayerTeam != null;
        }

        private void HandleRuntimeFault(string phase, Exception ex)
        {
            try
            {
                _runtimeFaulted = true;
                _commanderModeState.ForceDisabled($"runtime fault: {phase}");
                ModLogger.LogWarningOnce(
                    $"commander_mission_view_fault_{phase}",
                    $"CommanderMissionView {phase} faulted ({ex})");
                EnsureRuntimeFaultCleanup($"runtime fault: {phase}");
            }
            catch
            {
                // Never rethrow into Bannerlord.
            }
        }

        private void EnsureRuntimeFaultCleanup(string reason)
        {
            if (_runtimeFaultCleanupDone)
            {
                return;
            }

            _runtimeFaultCleanupDone = true;
            try
            {
                _nativeInputGuard.Cleanup();
                _backspaceConflictGuard.Cleanup();
                if (Mission != null)
                {
                    _cameraBridge.RestoreNativeCamera(Mission);
                }

                _commanderModeState.ForceDisabled(reason);
                _commandMarkerService?.Cleanup();
                _tacticalFeedback?.ResetSession();
                _commanderDiagnosticsService?.Cleanup();
                _troopAbsorptionController?.Clear();
                _twFormationSelectionState?.Clear();
                _cavalryDoctrineByFormation.Clear();
                _cavalryWideLayoutLogTime.Clear();
                _cavalrySequenceRegistry.Clear();
                _cavalryNativeSeqTransitionLog.Clear();
                _perfGate?.ResetAll();
                _perfDiagnostics?.Reset();
                _cameraController.Reset();
                _feedbackThrottle?.ClearKey("diagnostics-summary");
                _loggedFirstInternalPose = false;
                _loggedCameraBridgeNotAppliedWarning = false;
            }
            catch (Exception cleanupEx)
            {
                ModLogger.Warn($"{ModConstants.ModuleId}: runtime fault cleanup degraded ({cleanupEx})");
            }
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
