using Bannerlord.RTSCameraLite.Config;

namespace Bannerlord.RTSCameraLite.Doctrine
{
    /// <summary>
    /// Thresholds for formation behavior eligibility (Slice 11).
    /// </summary>
    public sealed class FormationEligibilitySettings
    {
        public float BasicLineMinimumDiscipline { get; set; }

        public float LooseMinimumDiscipline { get; set; }

        public float ShieldWallMinimumDiscipline { get; set; }

        public float SquareMinimumDiscipline { get; set; }

        public float CircleMinimumDiscipline { get; set; }

        public float AdvancedAdaptiveMinimumDiscipline { get; set; }

        public float MinimumShieldRatioForShieldWall { get; set; }

        public float MinimumPolearmOrShieldRatioForSquare { get; set; }

        public float MinimumMountedRatioForMountedWide { get; set; }

        public float MinimumHorseArcherRatioForHorseArcherLoose { get; set; }

        public bool NoCommanderAllowsBasicMobOrders { get; set; }

        public static FormationEligibilitySettings FromConfig(CommanderConfig config)
        {
            CommanderConfig c = config ?? CommanderConfigDefaults.CreateDefault();
            return new FormationEligibilitySettings
            {
                BasicLineMinimumDiscipline = c.BasicLineMinimumDiscipline,
                LooseMinimumDiscipline = c.LooseMinimumDiscipline,
                ShieldWallMinimumDiscipline = c.ShieldWallMinimumDiscipline,
                SquareMinimumDiscipline = c.SquareMinimumDiscipline,
                CircleMinimumDiscipline = c.CircleMinimumDiscipline,
                AdvancedAdaptiveMinimumDiscipline = c.AdvancedAdaptiveMinimumDiscipline,
                MinimumShieldRatioForShieldWall = c.MinimumShieldRatioForShieldWall,
                MinimumPolearmOrShieldRatioForSquare = c.MinimumPolearmOrShieldRatioForSquare,
                MinimumMountedRatioForMountedWide = c.MinimumMountedRatioForMountedWide,
                MinimumHorseArcherRatioForHorseArcherLoose = c.MinimumHorseArcherRatioForHorseArcherLoose,
                NoCommanderAllowsBasicMobOrders = c.NoCommanderAllowsBasicMobOrders
            };
        }
    }
}
