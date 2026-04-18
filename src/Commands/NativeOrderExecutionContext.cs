using Bannerlord.RTSCameraLite.Doctrine;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace Bannerlord.RTSCameraLite.Commands
{
    /// <summary>
    /// Immutable inputs for a single native primitive call (Slice 14 — all issuance flows through <see cref="NativeOrderPrimitiveExecutor"/>).
    /// </summary>
    public sealed class NativeOrderExecutionContext
    {
        public const string ReasonCavalrySequenceProbe = "cavalry_sequence_probe";

        public const string ReasonCavalrySequenceTick = "cavalry_sequence_tick";

        public NativeOrderExecutionContext(
            TaleWorlds.MountAndBlade.Mission mission,
            Formation sourceFormation,
            Formation targetFormation,
            Vec3? targetPosition,
            Vec2? targetDirection,
            string sourceReason,
            FormationEligibilityResult eligibility = null,
            Agent followTargetAgent = null)
        {
            Mission = mission;
            SourceFormation = sourceFormation;
            TargetFormation = targetFormation;
            TargetPosition = targetPosition;
            TargetDirection = targetDirection;
            SourceReason = sourceReason ?? string.Empty;
            Eligibility = eligibility;
            FollowTargetAgent = followTargetAgent;
        }

        public TaleWorlds.MountAndBlade.Mission Mission { get; }

        public Formation SourceFormation { get; }

        public Formation TargetFormation { get; }

        public Vec3? TargetPosition { get; }

        public Vec2? TargetDirection { get; }

        public string SourceReason { get; }

        /// <summary>When non-null and not <see cref="FormationEligibilityResult.Success"/>, executor must block (Slice 14 safety).</summary>
        public FormationEligibilityResult Eligibility { get; }

        /// <summary>Explicit follow target; when null, executor may fall back to <see cref="TaleWorlds.MountAndBlade.Mission.MainAgent"/>.</summary>
        public Agent FollowTargetAgent { get; }

        /// <summary>
        /// Doctrine/orchestrator calls: no router eligibility gate (eligibility null = not integrated yet).
        /// </summary>
        public static NativeOrderExecutionContext ForCavalryDoctrine(
            TaleWorlds.MountAndBlade.Mission mission,
            Formation sourceFormation,
            Formation targetFormation,
            Vec3? targetPosition,
            string sourceReason)
        {
            return new NativeOrderExecutionContext(
                mission,
                sourceFormation,
                targetFormation,
                targetPosition,
                null,
                sourceReason ?? string.Empty,
                eligibility: null,
                followTargetAgent: null);
        }

        public static NativeOrderExecutionContext FromRouter(
            CommandContext context,
            CommandIntent intent,
            NativeOrderPrimitive primitive)
        {
            Agent follow = null;
            if (primitive == NativeOrderPrimitive.FollowCommander
                && context?.Commander != null
                && context.Commander.HasCommander
                && context.Commander.Commander?.CommanderAgent != null)
            {
                follow = context.Commander.Commander.CommanderAgent;
            }

            return new NativeOrderExecutionContext(
                context.Mission,
                intent.SourceFormation,
                intent.TargetFormation,
                intent.TargetPosition,
                null,
                string.IsNullOrEmpty(intent.Source) ? context.SourceReason : intent.Source,
                context.Eligibility,
                follow);
        }
    }
}
