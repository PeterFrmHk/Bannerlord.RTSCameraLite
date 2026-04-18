using System;
using Bannerlord.RTSCameraLite.Adapters;
using Bannerlord.RTSCameraLite.Commander;
using Bannerlord.RTSCameraLite.Commands;
using Bannerlord.RTSCameraLite.Config;
using Bannerlord.RTSCameraLite.Equipment;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace Bannerlord.RTSCameraLite.Doctrine
{
    /// <summary>
    /// Native-order-driven cavalry charge sequence (Slice 16): assemble spacing context, advance/move, charge, lock release, disengage, reform.
    /// </summary>
    public sealed class CavalryNativeChargeOrchestrator
    {
        private const float NativeAdvanceReissueSeconds = 4f;

        private readonly CommanderConfig _config;
        private readonly FormationDataAdapter _adapter;
        private readonly NativeOrderPrimitiveExecutor _executor;
        private readonly CommanderAssignmentService _commanderAssignment;
        private readonly CavalrySequenceRegistry _registry;

        public CavalryNativeChargeOrchestrator(
            CommanderConfig config,
            FormationDataAdapter adapter,
            NativeOrderPrimitiveExecutor executor,
            CommanderAssignmentService commanderAssignment,
            CavalrySequenceRegistry registry)
        {
            _config = config ?? CommanderConfigDefaults.CreateDefault();
            _adapter = adapter ?? new FormationDataAdapter();
            _executor = executor ?? new NativeOrderPrimitiveExecutor(_config);
            _commanderAssignment = commanderAssignment ?? new CommanderAssignmentService(_adapter);
            _registry = registry ?? new CavalrySequenceRegistry();
        }

        public bool StartSequence(CommandIntent intent, CommandContext context, out string message)
        {
            message = string.Empty;
            try
            {
                if (!_config.EnableNativeCavalryChargeSequence)
                {
                    message = "native cavalry sequence disabled in config";
                    return false;
                }

                if (!_config.EnableNativePrimitiveOrderExecution)
                {
                    message = "native primitive order execution disabled in config";
                    return false;
                }

                if (!_config.EnableNativeOrderExecution)
                {
                    message = "EnableNativeOrderExecution is false";
                    return false;
                }

                if (context == null || !context.CommanderModeEnabled)
                {
                    message = "commander mode not enabled";
                    return false;
                }

                if (context.Commander == null || !context.Commander.HasCommander)
                {
                    message = "command requires a commander";
                    return false;
                }

                if (!IsCommanderAgentValid(context.Commander))
                {
                    message = "commander agent invalid";
                    return false;
                }

                if (intent == null || intent.SourceFormation == null)
                {
                    message = "intent or source formation is null";
                    return false;
                }

                TaleWorlds.MountAndBlade.Mission mission = context.Mission;
                if (mission == null)
                {
                    message = "mission is null";
                    return false;
                }

                Formation source = intent.SourceFormation;
                if (source.CountOfUnits <= 0)
                {
                    message = "source formation empty";
                    return false;
                }

                _registry.StopSequence(source);

                FormationCompositionProfile comp = FormationCompositionAnalyzer.Analyze(_adapter, source);
                if (!CavalrySpacingRules.IsCavalryHeavyFormation(comp))
                {
                    message = "formation is not cavalry-heavy";
                    return false;
                }

                Formation targetFormation = null;
                if (!CavalryTargetTracker.TryResolveTargetFormation(intent, out targetFormation))
                {
                    CavalryTargetTracker.TryGetNearestEnemyFormation(mission, source, _adapter, out targetFormation);
                }

                Vec3? lastKnown = null;
                if (!CavalryTargetTracker.TryResolveTargetPosition(
                        mission,
                        _adapter,
                        source,
                        intent,
                        targetFormation,
                        lastKnown,
                        out Vec3 targetWorld))
                {
                    message = "no target formation, position, or enemy fallback";
                    return false;
                }

                NativeOrderExecutionContext probeCtx = NativeOrderExecutionContext.ForCavalryDoctrine(
                    mission,
                    source,
                    targetFormation,
                    targetWorld,
                    NativeOrderExecutionContext.ReasonCavalrySequenceProbe);
                NativeOrderResult probe = _executor.ExecuteAdvanceOrMove(probeCtx);
                if (!probe.Executed)
                {
                    message = "native advance/move probe failed: " + probe.Message;
                    return false;
                }

                if (!CavalryTargetTracker.TryComputeDistanceToTarget(
                        source,
                        targetFormation,
                        targetWorld,
                        _adapter,
                        out float distToTarget))
                {
                    message = "could not compute distance to target";
                    return false;
                }

                var state = new CavalryChargeSequenceState(
                    source,
                    targetFormation,
                    CavalryChargeState.MountedFormationAssembling,
                    intent.TargetPosition ?? (Vec3?)targetWorld,
                    null,
                    positionLockReleased: false,
                    reformAllowed: false,
                    commanderValid: true,
                    distanceToTargetFormation: distToTarget,
                    distanceFromImpactPosition: float.MaxValue,
                    timeSinceLockRelease: 0f,
                    reason: "native cavalry sequence started")
                {
                    LastKnownTargetWorldPosition = targetWorld
                };

                return _registry.StartSequence(source, state);
            }
            catch (Exception ex)
            {
                message = "start suppressed: " + ex.Message;
                return false;
            }
        }

        public CavalrySequenceTickResult TickSequence(CavalryChargeSequenceState state, float dt)
        {
            NativeOrderResult lastNative = NativeOrderResult.Success(NativeOrderPrimitive.None, "tick");
            if (state == null || dt < 0f)
            {
                return new CavalrySequenceTickResult(false, false, true, CavalryChargeState.ChargeBroken, "null state", lastNative);
            }

            try
            {
                if (state.Aborted)
                {
                    return new CavalrySequenceTickResult(false, false, true, state.CurrentState, state.AbortReason, lastNative);
                }

                TaleWorlds.MountAndBlade.Mission mission = ResolveMission(state.SourceFormation);
                if (mission == null)
                {
                    AbortSequence(state, "mission unresolved");
                    return new CavalrySequenceTickResult(false, false, true, state.CurrentState, state.AbortReason, lastNative);
                }

                Formation source = state.SourceFormation;
                if (source == null || source.CountOfUnits <= 0)
                {
                    AbortSequence(state, "source formation invalid");
                    return new CavalrySequenceTickResult(false, false, true, state.CurrentState, state.AbortReason, lastNative);
                }

                CommanderPresenceResult presence = _commanderAssignment.DetectCommander(mission, source);
                bool commanderValid = IsCommanderAgentValid(presence);
                state.CommanderValid = commanderValid;
                if (!commanderValid)
                {
                    AbortSequence(state, "commander invalid or dead");
                    return new CavalrySequenceTickResult(false, false, true, CavalryChargeState.CommanderDead, state.AbortReason, lastNative);
                }

                FormationCompositionProfile comp = FormationCompositionAnalyzer.Analyze(_adapter, source);
                if (!CavalrySpacingRules.IsCavalryHeavyFormation(comp))
                {
                    AbortSequence(state, "no longer cavalry-heavy");
                    return new CavalrySequenceTickResult(false, false, true, state.CurrentState, state.AbortReason, lastNative);
                }

                bool horseArcherHeavy = CavalrySpacingRules.IsHorseArcherHeavyFormation(comp);
                bool shockHeavy = CavalrySpacingRules.IsShockCavalryHeavyFormation(comp, source);

                Formation targetFormation = state.TargetFormation;
                if (targetFormation != null && targetFormation.CountOfUnits <= 0)
                {
                    targetFormation = null;
                    state.TargetFormation = null;
                }

                var ghostIntent = new CommandIntent
                {
                    TargetFormation = targetFormation,
                    TargetPosition = state.TargetPosition
                };

                if (!CavalryTargetTracker.TryResolveTargetPosition(
                        mission,
                        _adapter,
                        source,
                        ghostIntent,
                        targetFormation,
                        state.LastKnownTargetWorldPosition ?? state.ImpactPosition,
                        out Vec3 targetWorld))
                {
                    AbortSequence(state, "target lost and no fallback position");
                    return new CavalrySequenceTickResult(false, false, true, state.CurrentState, state.AbortReason, lastNative);
                }

                state.LastKnownTargetWorldPosition = targetWorld;

                if (!CavalryTargetTracker.TryComputeDistanceToTarget(
                        source,
                        targetFormation,
                        targetWorld,
                        _adapter,
                        out float distToTarget))
                {
                    distToTarget = float.MaxValue;
                }

                state.DistanceToTargetFormation = distToTarget;

                if (!CavalryTargetTracker.TryComputeDistanceFromImpact(
                        source,
                        state.ImpactPosition,
                        targetWorld,
                        _adapter,
                        out float distFromImpact))
                {
                    distFromImpact = distToTarget;
                }

                state.DistanceFromImpactPosition = distFromImpact;

                state.SequenceTimeSeconds += dt;

                if (state.PositionLockReleased)
                {
                    state.TimeSinceLockRelease += dt;
                }
                else
                {
                    state.TimeSinceLockRelease = 0f;
                }

                float forwardDist = System.Math.Max(1f, _config.CavalryForwardToChargeDistance);

                if (!state.NativeChargeIssued
                    && distToTarget > forwardDist
                    && _config.CavalryUseNativeForwardBeforeCharge)
                {
                    bool firstAdvance = !state.NativeForwardIssued;
                    bool reissueAdvance = state.NativeForwardIssued
                        && (state.SequenceTimeSeconds - state.LastNativeAdvanceIssueTime >= NativeAdvanceReissueSeconds);
                    if (firstAdvance || reissueAdvance)
                    {
                        NativeOrderExecutionContext advCtx = NativeOrderExecutionContext.ForCavalryDoctrine(
                            mission,
                            source,
                            targetFormation,
                            targetWorld,
                            NativeOrderExecutionContext.ReasonCavalrySequenceTick);
                        NativeOrderResult adv = _executor.ExecuteAdvanceOrMove(advCtx);
                        lastNative = adv;
                        if (!adv.Executed)
                        {
                            AbortSequence(state, "native advance/move failed during sequence: " + adv.Message);
                            return new CavalrySequenceTickResult(false, false, true, state.CurrentState, state.AbortReason, lastNative);
                        }

                        state.NativeForwardIssued = true;
                        state.LastNativeAdvanceIssueTime = state.SequenceTimeSeconds;
                    }
                }

                if (!state.NativeChargeIssued && distToTarget <= forwardDist)
                {
                    state.CurrentState = CavalryChargeState.ChargeReady;
                }

                if (!state.NativeChargeIssued
                    && distToTarget <= forwardDist
                    && _config.CavalryUseNativeChargeCommand)
                {
                    NativeOrderExecutionContext chgCtx = NativeOrderExecutionContext.ForCavalryDoctrine(
                        mission,
                        source,
                        targetFormation,
                        null,
                        NativeOrderExecutionContext.ReasonCavalrySequenceTick);
                    NativeOrderResult chg = _executor.ExecuteCharge(chgCtx);
                    lastNative = chg;
                    if (!chg.Executed)
                    {
                        AbortSequence(state, "native charge failed during sequence: " + chg.Message);
                        return new CavalrySequenceTickResult(false, false, true, state.CurrentState, state.AbortReason, lastNative);
                    }

                    state.NativeChargeIssued = true;
                    state.CurrentState = CavalryChargeState.Charging;
                }

                CavalryImpactDetector.TryDetectImpactOrCloseContact(
                    source,
                    targetFormation,
                    targetWorld,
                    comp,
                    state.CurrentState,
                    distToTarget,
                    _adapter,
                    _config,
                    out bool closeContact,
                    out bool impactContact,
                    out string impactReason);

                bool releaseRecommended = CavalryPositionLockPolicy.ShouldReleasePositionLock(
                    distToTarget,
                    closeContact || impactContact,
                    shockHeavy && !horseArcherHeavy,
                    _config.CavalryReleaseLockDistance);

                if (releaseRecommended && !state.PositionLockReleased)
                {
                    state.PositionLockReleased = true;
                    state.CurrentState = CavalryChargeState.PositionLockReleased;
                    if (targetFormation != null)
                    {
                        FormationDataResult imp = _adapter.TryGetFormationCenter(targetFormation);
                        if (imp.Success)
                        {
                            state.ImpactPosition = imp.Vec3;
                        }
                        else
                        {
                            state.ImpactPosition = targetWorld;
                        }
                    }
                    else
                    {
                        state.ImpactPosition = targetWorld;
                    }

                    state.Reason = "lock released; " + impactReason;
                }

                if (state.PositionLockReleased
                    && state.NativeChargeIssued
                    && state.CurrentState == CavalryChargeState.PositionLockReleased)
                {
                    state.CurrentState = CavalryChargeState.Disengaging;
                }

                CavalryTargetTracker.TryComputeNearestEnemyFormationDistance(
                    mission,
                    source,
                    _adapter,
                    out float nearestEnemy,
                    out bool nearestKnown);

                bool reformDiscipline = CavalryReformPolicy.TryEvaluateReformDisciplineAllowed(
                    state.PositionLockReleased,
                    presence,
                    _config,
                    distToTarget,
                    distFromImpact,
                    nearestEnemy,
                    nearestKnown,
                    state.TimeSinceLockRelease,
                    out string reformReason);

                state.ReformAllowed = reformDiscipline;

                bool reactivateLock = CavalryPositionLockPolicy.ShouldReactivatePositionLock(
                    state.PositionLockReleased,
                    distToTarget,
                    distFromImpact,
                    state.TimeSinceLockRelease,
                    _config.CavalryReformDistanceFromAttackedFormation,
                    _config.CavalryReformCooldownSeconds,
                    reformDiscipline,
                    _config.CavalryReleaseLockDistance);

                if (reactivateLock && reformDiscipline && !state.ReformNativeIssued)
                {
                    FormationDataResult selfCenter = _adapter.TryGetFormationCenter(source);
                    Vec3 reformPos = selfCenter.Success ? selfCenter.Vec3 : targetWorld;
                    NativeOrderExecutionContext reformCtx = NativeOrderExecutionContext.ForCavalryDoctrine(
                        mission,
                        source,
                        targetFormation,
                        reformPos,
                        NativeOrderExecutionContext.ReasonCavalrySequenceTick);
                    NativeOrderResult reformResult = _executor.ExecuteReform(reformCtx);
                    lastNative = reformResult;
                    if (!reformResult.Executed)
                    {
                        AbortSequence(state, "native reform failed during sequence: " + reformResult.Message);
                        return new CavalrySequenceTickResult(false, false, true, state.CurrentState, state.AbortReason, lastNative);
                    }

                    state.ReformNativeIssued = true;
                    state.PositionLockReleased = false;
                    state.TimeSinceLockRelease = 0f;
                    state.CurrentState = CavalryChargeState.Reforming;
                    state.Reason = "reforming; " + reformReason;
                }

                if (state.ReformNativeIssued && state.CurrentState == CavalryChargeState.Reforming)
                {
                    state.CurrentState = CavalryChargeState.Reassembled;
                    state.Reason = "reassembled after native reform";
                    return new CavalrySequenceTickResult(
                        false,
                        completed: true,
                        aborted: false,
                        state.CurrentState,
                        "sequence complete",
                        lastNative);
                }

                state.Reason = $"tick ok; impact={impactReason}; reform={reformReason}";
                return new CavalrySequenceTickResult(true, false, false, state.CurrentState, state.Reason, lastNative);
            }
            catch (Exception ex)
            {
                AbortSequence(state, "tick threw: " + ex.Message);
                return new CavalrySequenceTickResult(false, false, true, state.CurrentState, state.AbortReason, lastNative);
            }
        }

        public void AbortSequence(CavalryChargeSequenceState state, string reason)
        {
            try
            {
                if (state == null)
                {
                    return;
                }

                state.Aborted = true;
                state.AbortReason = reason ?? string.Empty;
                if (reason != null && reason.IndexOf("commander", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    state.CurrentState = CavalryChargeState.CommanderDead;
                }
                else if (state.CurrentState != CavalryChargeState.CommanderDead)
                {
                    state.CurrentState = CavalryChargeState.ChargeBroken;
                }
            }
            catch
            {
                // never throw outward
            }
        }

        private static TaleWorlds.MountAndBlade.Mission ResolveMission(Formation formation)
        {
            if (formation == null)
            {
                return null;
            }

            try
            {
                Agent captain = formation.Captain;
                if (captain?.Mission != null)
                {
                    return captain.Mission;
                }

                TaleWorlds.MountAndBlade.Mission found = null;
                formation.ApplyActionOnEachUnit(a =>
                {
                    if (found == null && a?.Mission != null)
                    {
                        found = a.Mission;
                    }
                });

                if (found != null)
                {
                    return found;
                }
            }
            catch
            {
                // Fall through.
            }

            try
            {
                return TaleWorlds.MountAndBlade.Mission.Current;
            }
            catch
            {
                return null;
            }
        }

        private static bool IsCommanderAgentValid(CommanderPresenceResult presence)
        {
            try
            {
                if (presence == null || !presence.HasCommander || presence.Commander?.CommanderAgent == null)
                {
                    return false;
                }

                Agent a = presence.Commander.CommanderAgent;
                return a.IsActive() && a.Health > 0.01f;
            }
            catch
            {
                return false;
            }
        }
    }
}
