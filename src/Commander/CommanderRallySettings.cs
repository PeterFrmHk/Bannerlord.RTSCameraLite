using Bannerlord.RTSCameraLite.Config;

namespace Bannerlord.RTSCameraLite.Commander
{
    /// <summary>
    /// Radii and cadence for rally/absorption planning (Slice 12).
    /// </summary>
    public sealed class CommanderRallySettings
    {
        public float CommanderRallyRadius { get; set; }

        public float CommanderAbsorptionRadius { get; set; }

        public float FormationSlotRadius { get; set; }

        public float CohesionBreakRadius { get; set; }

        public float SlotReassignmentCooldownSeconds { get; set; }

        public float RallyScanIntervalSeconds { get; set; }

        public static CommanderRallySettings FromConfig(CommanderConfig config)
        {
            CommanderConfig c = config ?? CommanderConfigDefaults.CreateDefault();
            float rally = MathMax(1f, c.CommanderRallyRadius);
            float absorption = MathMax(0.5f, MathMinFloat(c.CommanderAbsorptionRadius, rally));
            float cohesion = MathMax(rally + 1f, c.CohesionBreakRadius);
            return new CommanderRallySettings
            {
                CommanderRallyRadius = rally,
                CommanderAbsorptionRadius = absorption,
                FormationSlotRadius = MathMax(0.25f, c.FormationSlotRadius),
                CohesionBreakRadius = cohesion,
                SlotReassignmentCooldownSeconds = MathMax(0.1f, c.SlotReassignmentCooldownSeconds),
                RallyScanIntervalSeconds = MathMax(0.5f, c.RallyScanIntervalSeconds)
            };
        }

        private static float MathMax(float a, float b)
        {
            return a > b ? a : b;
        }

        private static float MathMinFloat(float a, float b)
        {
            return a < b ? a : b;
        }
    }
}
