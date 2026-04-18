using System;
using Bannerlord.RTSCameraLite.Config;
using Bannerlord.RTSCameraLite.Doctrine;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace Bannerlord.RTSCameraLite.Commands
{
    /// <summary>
    /// Validates intents and optionally forwards to <see cref="NativeOrderPrimitiveExecutor"/> when enabled (Slice 15).
    /// </summary>
    public sealed class CommandRouter
    {
        private readonly CommanderConfig _config;
        private readonly FormationRestrictionService _restrictions;
        private readonly NativeOrderPrimitiveExecutor _nativePrimitives;

        public CommandRouter(CommanderConfig config, NativeOrderPrimitiveExecutor nativePrimitives = null)
        {
            _config = config ?? CommanderConfigDefaults.CreateDefault();
            _restrictions = new FormationRestrictionService(_config);
            _nativePrimitives = nativePrimitives ?? new NativeOrderPrimitiveExecutor(_config);
        }

        public CommandValidationResult Validate(CommandIntent intent, CommandContext context)
        {
            try
            {
                if (!_config.EnableCommandRouter)
                {
                    return CommandValidationResult.Invalid("command router disabled in config", intent);
                }

                if (intent == null)
                {
                    return CommandValidationResult.Invalid("intent is null");
                }

                if (context == null)
                {
                    return CommandValidationResult.Invalid("context is null", intent);
                }

                if (intent.Type == CommandType.None)
                {
                    return CommandValidationResult.Valid("none", intent);
                }

                if (!context.CommanderModeEnabled)
                {
                    return CommandValidationResult.Blocked("commander mode is not enabled", intent);
                }

                if (context.Mission == null)
                {
                    return CommandValidationResult.Blocked("mission is null", intent);
                }

                if (intent.RequiresCommander
                    && (context.Commander == null || !context.Commander.HasCommander))
                {
                    return CommandValidationResult.Invalid("command requires a commander", intent);
                }

                if (intent.SourceFormation == null)
                {
                    return CommandValidationResult.Invalid("source formation is null", intent);
                }

                if (intent.RequiresPosition)
                {
                    if (!intent.TargetPosition.HasValue)
                    {
                        return CommandValidationResult.Invalid("target position is required", intent);
                    }

                    if (!IsFiniteVec3(intent.TargetPosition.Value))
                    {
                        return CommandValidationResult.Invalid("target position is not finite", intent);
                    }
                }

                if (intent.RequiresTargetFormation && intent.TargetFormation == null)
                {
                    return CommandValidationResult.Invalid("target formation is required", intent);
                }

                RestrictionDecision restriction = _restrictions.Evaluate(intent, context);
                if (restriction.Blocked)
                {
                    return CommandValidationResult.Blocked(restriction.Reason, intent);
                }

                if (!restriction.Allowed)
                {
                    return CommandValidationResult.Invalid(restriction.Reason, intent);
                }

                return CommandValidationResult.Valid(
                    string.IsNullOrEmpty(intent.Source) ? "validated" : intent.Source,
                    intent);
            }
            catch (Exception ex)
            {
                return CommandValidationResult.Invalid("validate threw: " + ex.Message, intent);
            }
        }

        public CommandExecutionDecision Decide(CommandIntent intent, CommandContext context)
        {
            CommandValidationResult validation = Validate(intent, context);
            if (!validation.IsValid || validation.IsBlocked)
            {
                return new CommandExecutionDecision(
                    false,
                    false,
                    false,
                    NativeOrderPrimitive.None,
                    validation.Message);
            }

            if (intent != null && intent.Type == CommandType.NativeCavalryChargeSequence)
            {
                return new CommandExecutionDecision(
                    false,
                    requiresNativeOrder: false,
                    requiresCavalrySequence: true,
                    NativeOrderPrimitive.None,
                    "native cavalry charge sequence is handled by CavalryNativeChargeOrchestrator (not direct primitives)");
            }

            bool canRunNative = _config.EnableNativePrimitiveOrderExecution;
            MapPrimitives(intent, out NativeOrderPrimitive primitive, out bool requiresCavalrySequence, out bool requiresNative);

            bool shouldExecute = canRunNative && requiresNative && primitive != NativeOrderPrimitive.None;
            string reason = shouldExecute
                ? "native primitive execution enabled"
                : (!canRunNative
                    ? (!_config.EnableNativeOrderExecution
                        ? "EnableNativeOrderExecution is false"
                        : "EnableNativePrimitiveOrderExecution is false")
                    : "no mapped primitive for this command");

            if (shouldExecute)
            {
                TryInvokePrimitive(intent, context, primitive);
            }

            return new CommandExecutionDecision(
                shouldExecute,
                requiresNative,
                requiresCavalrySequence,
                primitive,
                reason);
        }

        private void TryInvokePrimitive(CommandIntent intent, CommandContext context, NativeOrderPrimitive primitive)
        {
            if (intent?.SourceFormation == null || context == null)
            {
                return;
            }

            try
            {
                NativeOrderExecutionContext ctx = NativeOrderExecutionContext.FromRouter(context, intent, primitive);
                switch (primitive)
                {
                    case NativeOrderPrimitive.AdvanceOrMove:
                        _nativePrimitives.ExecuteAdvanceOrMove(ctx);
                        break;
                    case NativeOrderPrimitive.Charge:
                        _nativePrimitives.ExecuteCharge(ctx);
                        break;
                    case NativeOrderPrimitive.Hold:
                        _nativePrimitives.ExecuteHold(ctx);
                        break;
                    case NativeOrderPrimitive.Reform:
                        _nativePrimitives.ExecuteReform(ctx);
                        break;
                    case NativeOrderPrimitive.FollowCommander:
                        _nativePrimitives.ExecuteFollowCommander(ctx);
                        break;
                    case NativeOrderPrimitive.Stop:
                        _nativePrimitives.ExecuteStop(ctx);
                        break;
                }
            }
            catch
            {
                // Executor already defensive; never throw to callers.
            }
        }

        private static void MapPrimitives(
            CommandIntent intent,
            out NativeOrderPrimitive primitive,
            out bool requiresCavalrySequence,
            out bool requiresNative)
        {
            requiresCavalrySequence = false;
            requiresNative = false;
            primitive = NativeOrderPrimitive.None;

            switch (intent.Type)
            {
                case CommandType.AdvanceOrMove:
                    primitive = NativeOrderPrimitive.AdvanceOrMove;
                    requiresNative = intent.TargetPosition.HasValue;
                    break;
                case CommandType.Charge:
                    primitive = NativeOrderPrimitive.Charge;
                    requiresNative = true;
                    break;
                case CommandType.BasicHold:
                    primitive = NativeOrderPrimitive.Hold;
                    requiresNative = true;
                    break;
                case CommandType.Reform:
                    primitive = NativeOrderPrimitive.Reform;
                    requiresNative = true;
                    break;
                case CommandType.NativeCavalryChargeSequence:
                    requiresCavalrySequence = true;
                    primitive = NativeOrderPrimitive.None;
                    requiresNative = false;
                    break;
                default:
                    primitive = NativeOrderPrimitive.None;
                    requiresNative = false;
                    break;
            }
        }

        private static bool IsFiniteVec3(Vec3 v)
        {
            return !(float.IsNaN(v.x) || float.IsInfinity(v.x)
                || float.IsNaN(v.y) || float.IsInfinity(v.y)
                || float.IsNaN(v.z) || float.IsInfinity(v.z));
        }

        /// <summary>Validates and starts the Slice 16 orchestrator (does not invoke charge primitive directly).</summary>
        public bool TryStartNativeCavalryChargeSequence(
            CommandIntent intent,
            CommandContext context,
            CavalryNativeChargeOrchestrator orchestrator,
            out string message)
        {
            message = string.Empty;
            try
            {
                if (orchestrator == null)
                {
                    message = "orchestrator is null";
                    return false;
                }

                if (!_config.EnableNativeCavalryChargeSequence)
                {
                    message = "EnableNativeCavalryChargeSequence is false";
                    return false;
                }

                if (!_config.EnableNativePrimitiveOrderExecution)
                {
                    message = "EnableNativePrimitiveOrderExecution is false";
                    return false;
                }

                if (!_config.EnableNativeOrderExecution)
                {
                    message = "EnableNativeOrderExecution is false";
                    return false;
                }

                CommandValidationResult validation = Validate(intent, context);
                if (!validation.IsValid || validation.IsBlocked)
                {
                    message = validation.Message;
                    return false;
                }

                if (intent == null || intent.Type != CommandType.NativeCavalryChargeSequence)
                {
                    message = "intent is not NativeCavalryChargeSequence";
                    return false;
                }

                return orchestrator.StartSequence(intent, context, out message);
            }
            catch (Exception ex)
            {
                message = "TryStartNativeCavalryChargeSequence: " + ex.Message;
                return false;
            }
        }
    }
}
