using System;
using System.Collections.Generic;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace Bannerlord.RTSCameraLite.Commands
{
    /// <summary>
    /// Issues a minimal set of native formation orders via public <see cref="OrderController"/> APIs.
    /// Validation is performed by <see cref="CommandRouter"/> before <see cref="Execute"/> is called.
    /// </summary>
    internal sealed class NativeOrderExecutor
    {
        public CommandExecutionResult Execute(CommandIntent intent, CommandContext context)
        {
            try
            {
                if (intent == null)
                {
                    return CommandExecutionResult.Failure(CommandType.None, "No command intent.");
                }

                if (context?.Mission == null)
                {
                    return CommandExecutionResult.Failure(intent.Type, "No active mission.");
                }

                TaleWorlds.MountAndBlade.Mission mission = context.Mission;
                if (mission.MissionEnded)
                {
                    return CommandExecutionResult.Failure(intent.Type, "Mission has ended.");
                }

                switch (intent.Type)
                {
                    case CommandType.Charge:
                        return IssueMovementOrderType(mission, intent, OrderType.Charge);
                    case CommandType.HoldPosition:
                        return IssueMovementOrderType(mission, intent, OrderType.StandYourGround);
                    case CommandType.MoveToPosition:
                        return IssueMoveToPosition(mission, intent);
                    default:
                        return CommandExecutionResult.Failure(
                            intent.Type,
                            "Unsupported command type for native execution.");
                }
            }
            catch (Exception ex)
            {
                CommandType t = intent?.Type ?? CommandType.None;
                return CommandExecutionResult.Failure(t, $"Order execution error: {ex.Message}");
            }
        }

        private static CommandExecutionResult IssueMoveToPosition(
            TaleWorlds.MountAndBlade.Mission mission,
            CommandIntent intent)
        {
            Formation formation = intent.TargetFormation;
            if (formation == null)
            {
                return CommandExecutionResult.Failure(CommandType.MoveToPosition, "No target formation.");
            }

            if (!intent.TargetPosition.HasValue)
            {
                return CommandExecutionResult.Failure(CommandType.MoveToPosition, "No target position.");
            }

            Scene scene = mission.Scene;
            if (scene == null)
            {
                return CommandExecutionResult.Failure(CommandType.MoveToPosition, "Mission scene is not available.");
            }

            Vec3 p = intent.TargetPosition.Value;
            WorldPosition worldPosition = new WorldPosition(scene, p);
            return IssueWithOrderController(
                mission,
                formation,
                oc => oc.SetOrderWithPosition(OrderType.Move, worldPosition),
                CommandType.MoveToPosition,
                "Move order issued.",
                p);
        }

        private static CommandExecutionResult IssueMovementOrderType(
            TaleWorlds.MountAndBlade.Mission mission,
            CommandIntent intent,
            OrderType orderType)
        {
            Formation formation = intent.TargetFormation;
            if (formation == null)
            {
                return CommandExecutionResult.Failure(intent.Type, "No target formation.");
            }

            return IssueWithOrderController(
                mission,
                formation,
                oc => oc.SetOrder(orderType),
                intent.Type,
                $"{orderType} order issued.");
        }

        private static CommandExecutionResult IssueWithOrderController(
            TaleWorlds.MountAndBlade.Mission mission,
            Formation formation,
            Action<OrderController> issue,
            CommandType commandType,
            string successMessage,
            Vec3? markerWorldPosition = null)
        {
            Agent main = mission.MainAgent;
            if (main == null)
            {
                return CommandExecutionResult.Failure(commandType, "No main agent (player leader).");
            }

            Team team = main.Team;
            if (team == null)
            {
                return CommandExecutionResult.Failure(commandType, "Main agent has no team.");
            }

            OrderController controller = team.GetOrderControllerOf(main) ?? team.PlayerOrderController;
            if (controller == null)
            {
                return CommandExecutionResult.Failure(commandType, "No order controller for the player team.");
            }

            bool selectable;
            try
            {
                selectable = controller.IsFormationSelectable(formation);
            }
            catch
            {
                selectable = false;
            }

            if (!selectable)
            {
                return CommandExecutionResult.Failure(commandType, "Formation is not selectable for orders.");
            }

            MBReadOnlyList<Formation> previous = controller.SelectedFormations;
            var snapshot = new List<Formation>(previous.Count);
            for (int i = 0; i < previous.Count; i++)
            {
                Formation f = previous[i];
                if (f != null)
                {
                    snapshot.Add(f);
                }
            }

            try
            {
                controller.ClearSelectedFormations();
                controller.SelectFormation(formation);
                issue(controller);
                return CommandExecutionResult.Success(commandType, successMessage, markerWorldPosition);
            }
            catch (Exception ex)
            {
                return CommandExecutionResult.Failure(commandType, ex.Message);
            }
            finally
            {
                try
                {
                    controller.ClearSelectedFormations();
                    for (int i = 0; i < snapshot.Count; i++)
                    {
                        Formation f = snapshot[i];
                        if (f != null)
                        {
                            controller.SelectFormation(f);
                        }
                    }
                }
                catch
                {
                    // Never throw from cleanup; selection state is best-effort.
                }
            }
        }
    }
}
