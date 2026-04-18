using System;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace Bannerlord.RTSCameraLite.Commands
{
    /// <summary>
    /// Validates <see cref="CommandIntent"/> against <see cref="CommandContext"/>.
    /// Native issuance is handled by <see cref="NativeOrderExecutor"/> after successful validation (slice 12).
    /// </summary>
    internal sealed class CommandRouter
    {
        public CommandExecutionResult ExecuteValidated(
            CommandIntent intent,
            CommandContext context,
            NativeOrderExecutor executor)
        {
            if (executor == null)
            {
                return CommandExecutionResult.Failure(
                    intent?.Type ?? CommandType.None,
                    "No order executor.");
            }

            CommandValidationResult validation = Validate(intent, context);
            if (!validation.IsValid)
            {
                return CommandExecutionResult.Failure(intent?.Type ?? CommandType.None, validation.Message);
            }

            return executor.Execute(intent, context);
        }

        public CommandValidationResult Validate(CommandIntent intent, CommandContext context)
        {
            try
            {
                if (intent == null)
                {
                    return CommandValidationResult.Invalid("No command intent.");
                }

                if (context == null)
                {
                    return CommandValidationResult.Invalid("No command context.");
                }

                if (!context.RtsModeEnabled)
                {
                    return CommandValidationResult.Invalid("RTS mode is off.");
                }

                if (context.Mission == null)
                {
                    return CommandValidationResult.Invalid("No active mission.");
                }

                if (intent.Type == CommandType.None)
                {
                    return CommandValidationResult.Valid("No command (None).");
                }

                if (intent.TargetFormation == null || !IsFormationUsable(intent.TargetFormation))
                {
                    return CommandValidationResult.Invalid("No valid formation selected for this command.");
                }

                if (context.Mission.MainAgent == null)
                {
                    return CommandValidationResult.Invalid("No player main agent for tactical orders.");
                }

                if (!ReferenceEquals(intent.TargetFormation.Team, context.Mission.MainAgent.Team))
                {
                    return CommandValidationResult.Invalid("Formation is not on the player's team.");
                }

                if (intent.RequiresPosition)
                {
                    if (!intent.TargetPosition.HasValue)
                    {
                        return CommandValidationResult.Invalid("This command requires a target position.");
                    }

                    if (!IsFiniteVec3(intent.TargetPosition.Value))
                    {
                        return CommandValidationResult.Invalid("Target position is not usable.");
                    }
                }

                if (intent.RequiresDirection)
                {
                    if (!intent.TargetDirection.HasValue)
                    {
                        return CommandValidationResult.Invalid("This command requires a direction.");
                    }

                    if (!IsFiniteVec2(intent.TargetDirection.Value))
                    {
                        return CommandValidationResult.Invalid("Target direction is not usable.");
                    }
                }

                return CommandValidationResult.Valid(
                    $"{intent.Type} for selected formation passed validation ({intent.Source}).");
            }
            catch (Exception ex)
            {
                return CommandValidationResult.Invalid($"Validation error: {ex.Message}");
            }
        }

        private static bool IsFormationUsable(Formation formation)
        {
            if (formation == null)
            {
                return false;
            }

            try
            {
                return formation.CountOfUnits > 0;
            }
            catch
            {
                return false;
            }
        }

        private static bool IsFiniteVec3(Vec3 v)
        {
            return !(float.IsNaN(v.x) || float.IsInfinity(v.x)
                || float.IsNaN(v.y) || float.IsInfinity(v.y)
                || float.IsNaN(v.z) || float.IsInfinity(v.z));
        }

        private static bool IsFiniteVec2(Vec2 v)
        {
            return !(float.IsNaN(v.x) || float.IsInfinity(v.x)
                || float.IsNaN(v.y) || float.IsInfinity(v.y));
        }
    }
}
