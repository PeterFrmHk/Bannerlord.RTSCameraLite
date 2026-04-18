using Bannerlord.RTSCameraLite.Commander;
using Bannerlord.RTSCameraLite.Doctrine;
using TaleWorlds.MountAndBlade;

namespace Bannerlord.RTSCameraLite.Commands
{
    /// <summary>Mission and doctrine/eligibility snapshot for validating a <see cref="CommandIntent"/> (Slice 15).</summary>
    public sealed class CommandContext
    {
        public CommandContext(
            TaleWorlds.MountAndBlade.Mission mission,
            bool commanderModeEnabled,
            CommanderPresenceResult commander,
            FormationDoctrineProfile doctrineProfile,
            FormationEligibilityResult eligibility,
            string sourceReason)
        {
            Mission = mission;
            CommanderModeEnabled = commanderModeEnabled;
            Commander = commander;
            DoctrineProfile = doctrineProfile;
            Eligibility = eligibility;
            SourceReason = sourceReason ?? string.Empty;
        }

        public TaleWorlds.MountAndBlade.Mission Mission { get; }

        public bool CommanderModeEnabled { get; }

        public CommanderPresenceResult Commander { get; }

        public FormationDoctrineProfile DoctrineProfile { get; }

        public FormationEligibilityResult Eligibility { get; }

        public string SourceReason { get; }
    }
}
