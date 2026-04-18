using System;
using System.Collections.Generic;
using Bannerlord.RTSCameraLite.Config;
using Bannerlord.RTSCameraLite.Core;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace Bannerlord.RTSCameraLite.Commands
{
    /// <summary>
    /// Single adapter boundary for TaleWorlds order primitives (Slice 14).
    /// When <see cref="CommanderConfig.EnableNativeOrderExecution"/> is false, all entry points return <see cref="NativeOrderResult.BlockedResult"/>.
    /// When true, uses public <see cref="OrderController"/> APIs documented in <c>docs/research/base-game-order-scan.md</c> and <c>docs/research/native-order-hooks.md</c> (pinned ref 1.2.12.66233).
    /// </summary>
    public sealed class NativeOrderPrimitiveExecutor
    {
        private static long _lastNativeSuccessDebugMs;

        private readonly CommanderConfig _config;

        public NativeOrderPrimitiveExecutor(CommanderConfig config = null)
        {
            _config = config ?? CommanderConfigDefaults.CreateDefault();
        }

        public NativeOrderResult ExecuteAdvanceOrMove(NativeOrderExecutionContext context)
        {
            if (context?.TargetPosition == null)
            {
                return NativeOrderResult.Failure(NativeOrderPrimitive.AdvanceOrMove, "TargetPosition is required for AdvanceOrMove");
            }

            return Execute(context, NativeOrderPrimitive.AdvanceOrMove, _config.AllowNativeAdvanceOrMove, IssueAdvanceOrMove);
        }

        public NativeOrderResult ExecuteCharge(NativeOrderExecutionContext context)
        {
            return Execute(context, NativeOrderPrimitive.Charge, _config.AllowNativeCharge, IssueCharge);
        }

        public NativeOrderResult ExecuteHold(NativeOrderExecutionContext context)
        {
            return Execute(context, NativeOrderPrimitive.Hold, _config.AllowNativeHold, IssueHold);
        }

        public NativeOrderResult ExecuteReform(NativeOrderExecutionContext context)
        {
            if (context?.TargetPosition == null)
            {
                return NativeOrderResult.Failure(NativeOrderPrimitive.Reform, "TargetPosition is required for Reform");
            }

            return Execute(context, NativeOrderPrimitive.Reform, _config.AllowNativeReform, IssueReform);
        }

        public NativeOrderResult ExecuteFollowCommander(NativeOrderExecutionContext context)
        {
            return Execute(context, NativeOrderPrimitive.FollowCommander, _config.AllowNativeFollowCommander, IssueFollowCommander);
        }

        public NativeOrderResult ExecuteStop(NativeOrderExecutionContext context)
        {
            return Execute(context, NativeOrderPrimitive.Stop, _config.AllowNativeStop, IssueStop);
        }

        private delegate void IssueDelegate(OrderController controller, NativeOrderExecutionContext ctx);

        private NativeOrderResult Execute(
            NativeOrderExecutionContext context,
            NativeOrderPrimitive primitive,
            bool primitiveAllowed,
            IssueDelegate issue)
        {
            try
            {
                if (context == null)
                {
                    return NativeOrderResult.Failure(primitive, "context is null");
                }

                if (context.Mission == null)
                {
                    return NativeOrderResult.Failure(primitive, "mission is null");
                }

                if (context.Mission.MissionEnded)
                {
                    return NativeOrderResult.BlockedResult(primitive, "mission ended");
                }

                if (context.SourceFormation == null)
                {
                    return NativeOrderResult.Failure(primitive, "source formation is null");
                }

                if (!_config.EnableNativeOrderExecution)
                {
                    MaybeLogExecutionDisabled(primitive);
                    return NativeOrderResult.BlockedResult(primitive, "native order execution disabled in config");
                }

                if (!primitiveAllowed)
                {
                    return NativeOrderResult.BlockedResult(primitive, "primitive disabled in config");
                }

                if (context.Eligibility != null && !context.Eligibility.Success)
                {
                    return NativeOrderResult.BlockedResult(primitive, "formation eligibility did not approve this behavior");
                }

                if (!IsPlayerOwnedFormation(context.Mission, context.SourceFormation, out string teamReason))
                {
                    return NativeOrderResult.BlockedResult(primitive, teamReason);
                }

                if (!TryResolveOrderController(context.Mission, out OrderController controller, out string ocReason))
                {
                    return NativeOrderResult.Failure(primitive, ocReason);
                }

                if (!controller.IsFormationSelectable(context.SourceFormation))
                {
                    return NativeOrderResult.BlockedResult(primitive, "formation is not selectable");
                }

                using (new OrderSelectionScope(controller, context.SourceFormation, _config.EnableNativeOrderDebug))
                {
                    issue(controller, context);
                }

                MaybeLogSuccess(primitive);
                return NativeOrderResult.Success(primitive, context.SourceReason);
            }
            catch (Exception ex)
            {
                ModLogger.LogDebug($"{ModConstants.ModuleId}: NativeOrderPrimitiveExecutor.{primitive} suppressed: {ex.Message}");
                return NativeOrderResult.Failure(primitive, ex.Message);
            }
        }

        private static void IssueAdvanceOrMove(OrderController controller, NativeOrderExecutionContext ctx)
        {
            if (!ctx.TargetPosition.HasValue)
            {
                throw new InvalidOperationException("AdvanceOrMove requires TargetPosition");
            }

            Vec3 p = ctx.TargetPosition.Value;
            if (float.IsNaN(p.x) || float.IsInfinity(p.x)
                || float.IsNaN(p.y) || float.IsInfinity(p.y)
                || float.IsNaN(p.z) || float.IsInfinity(p.z))
            {
                throw new InvalidOperationException("TargetPosition is not finite");
            }

            if (ctx.Mission.Scene == null)
            {
                throw new InvalidOperationException("mission scene is null");
            }

            WorldPosition wp = new WorldPosition(ctx.Mission.Scene, p);
            controller.SetOrderWithPosition(OrderType.Move, wp);
        }

        private static void IssueCharge(OrderController controller, NativeOrderExecutionContext ctx)
        {
            if (ctx.TargetFormation != null)
            {
                controller.SetOrderWithFormation(OrderType.ChargeWithTarget, ctx.TargetFormation);
            }
            else
            {
                controller.SetOrder(OrderType.Charge);
            }
        }

        private static void IssueHold(OrderController controller, NativeOrderExecutionContext ctx)
        {
            controller.SetOrder(OrderType.StandYourGround);
        }

        private static void IssueReform(OrderController controller, NativeOrderExecutionContext ctx)
        {
            if (!ctx.TargetPosition.HasValue)
            {
                throw new InvalidOperationException("Reform requires TargetPosition (positional reform anchor)");
            }

            Vec3 p = ctx.TargetPosition.Value;
            if (ctx.Mission.Scene == null)
            {
                throw new InvalidOperationException("mission scene is null");
            }

            WorldPosition wp = new WorldPosition(ctx.Mission.Scene, p);
            controller.SetOrderWithPosition(OrderType.Move, wp);
        }

        private static void IssueFollowCommander(OrderController controller, NativeOrderExecutionContext ctx)
        {
            Agent target = ctx.FollowTargetAgent ?? ctx.Mission.MainAgent;
            if (target == null)
            {
                throw new InvalidOperationException("no follow target agent");
            }

            controller.SetOrderWithAgent(OrderType.FollowMe, target);
        }

        private static void IssueStop(OrderController controller, NativeOrderExecutionContext ctx)
        {
            // No OrderType.Stop on pinned ref; StandYourGround halts advance (see slice doc).
            controller.SetOrder(OrderType.StandYourGround);
        }

        private static bool TryResolveOrderController(TaleWorlds.MountAndBlade.Mission mission, out OrderController controller, out string error)
        {
            controller = null;
            error = string.Empty;
            try
            {
                Team team = mission.PlayerTeam;
                if (team == null)
                {
                    error = "player team is null";
                    return false;
                }

                if (mission.MainAgent != null)
                {
                    controller = team.GetOrderControllerOf(mission.MainAgent);
                }

                controller = controller ?? team.PlayerOrderController;
                if (controller == null)
                {
                    error = "no OrderController for player";
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                error = "order controller resolve: " + ex.Message;
                return false;
            }
        }

        private static bool IsPlayerOwnedFormation(TaleWorlds.MountAndBlade.Mission mission, Formation formation, out string reason)
        {
            reason = string.Empty;
            try
            {
                Team team = mission?.PlayerTeam;
                if (team == null)
                {
                    reason = "player team is null";
                    return false;
                }

                foreach (Formation f in team.FormationsIncludingEmpty)
                {
                    if (f == formation)
                    {
                        return true;
                    }
                }

                reason = "source formation is not on the player team";
                return false;
            }
            catch (Exception ex)
            {
                reason = "team check threw: " + ex.Message;
                return false;
            }
        }

        private void MaybeLogExecutionDisabled(NativeOrderPrimitive primitive)
        {
            if (!_config.EnableNativeOrderDebug)
            {
                return;
            }

            ModLogger.LogWarningOnce(
                "native_order_executor_disabled",
                $"{ModConstants.ModuleId}: Native primitive {primitive} blocked (EnableNativeOrderExecution is false).");
        }

        private void MaybeLogSuccess(NativeOrderPrimitive primitive)
        {
            if (!_config.EnableNativeOrderDebug)
            {
                return;
            }

            long nowMs = DateTime.UtcNow.Ticks / TimeSpan.TicksPerMillisecond;
            if (nowMs - _lastNativeSuccessDebugMs < 2000)
            {
                return;
            }

            _lastNativeSuccessDebugMs = nowMs;
            ModLogger.LogDebug($"{ModConstants.ModuleId}: Native primitive issued: {primitive} (OrderController path).");
        }

        /// <summary>
        /// Restores prior <see cref="OrderController"/> selection after issuing a player-style order.
        /// </summary>
        private sealed class OrderSelectionScope : IDisposable
        {
            private readonly OrderController _controller;
            private readonly List<Formation> _snapshot = new List<Formation>();
            private readonly bool _log;

            public OrderSelectionScope(OrderController controller, Formation source, bool log)
            {
                _controller = controller;
                _log = log;
                try
                {
                    if (controller.SelectedFormations != null)
                    {
                        foreach (Formation f in controller.SelectedFormations)
                        {
                            if (f != null)
                            {
                                _snapshot.Add(f);
                            }
                        }
                    }

                    controller.ClearSelectedFormations();
                    controller.SelectFormation(source);
                }
                catch (Exception ex)
                {
                    if (_log)
                    {
                        ModLogger.LogDebug($"{ModConstants.ModuleId}: OrderSelectionScope init: {ex.Message}");
                    }
                }
            }

            public void Dispose()
            {
                try
                {
                    _controller.ClearSelectedFormations();
                    for (int i = 0; i < _snapshot.Count; i++)
                    {
                        Formation f = _snapshot[i];
                        if (f != null && _controller.IsFormationSelectable(f))
                        {
                            _controller.SelectFormation(f);
                        }
                    }
                }
                catch (Exception ex)
                {
                    if (_log)
                    {
                        ModLogger.LogDebug($"{ModConstants.ModuleId}: OrderSelectionScope restore: {ex.Message}");
                    }
                }
            }
        }
    }
}
