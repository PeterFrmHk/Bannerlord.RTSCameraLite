using Bannerlord.RTSCameraLite.Config;

namespace Bannerlord.RTSCameraLite.Commander
{
    /// <summary>
    /// Policy gates for who may count as a formation commander (Slice 8 — detection only).
    /// </summary>
    public sealed class CommanderDetectionSettings
    {
        public bool RequireHeroCommanderForAdvancedFormations { get; set; }

        public bool AllowCaptainCommander { get; set; }

        public bool AllowSergeantFallback { get; set; }

        public bool AllowHighestTierFallback { get; set; }

        public bool NoCommanderAllowsBasicMobOrders { get; set; }

        /// <summary>Threshold in 0..1; values outside range are clamped when building settings from config.</summary>
        public float MinimumCommandAuthorityScore { get; set; }

        public static CommanderDetectionSettings FromConfig(CommanderConfig config)
        {
            CommanderConfig c = config ?? CommanderConfigDefaults.CreateDefault();
            return new CommanderDetectionSettings
            {
                RequireHeroCommanderForAdvancedFormations = c.RequireHeroCommanderForAdvancedFormations,
                AllowCaptainCommander = c.AllowCaptainCommander,
                AllowSergeantFallback = c.AllowSergeantFallback,
                AllowHighestTierFallback = c.AllowHighestTierFallback,
                NoCommanderAllowsBasicMobOrders = c.NoCommanderAllowsBasicMobOrders,
                MinimumCommandAuthorityScore = Clamp01(c.MinimumCommandAuthorityScore)
            };
        }

        private static float Clamp01(float v)
        {
            if (v < 0f)
            {
                return 0f;
            }

            if (v > 1f)
            {
                return 1f;
            }

            return v;
        }
    }
}
