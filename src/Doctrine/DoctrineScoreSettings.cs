using Bannerlord.RTSCameraLite.Config;

namespace Bannerlord.RTSCameraLite.Doctrine
{
    /// <summary>Weights for doctrine discipline synthesis (Slice 10).</summary>
    public sealed class DoctrineScoreSettings
    {
        public DoctrineScoreSettings(
            float moraleWeight,
            float trainingWeight,
            float equipmentWeight,
            float commanderWeight,
            float cohesionWeight,
            float rankWeight,
            float casualtyShockPenaltyWeight)
        {
            MoraleWeight = moraleWeight;
            TrainingWeight = trainingWeight;
            EquipmentWeight = equipmentWeight;
            CommanderWeight = commanderWeight;
            CohesionWeight = cohesionWeight;
            RankWeight = rankWeight;
            CasualtyShockPenaltyWeight = casualtyShockPenaltyWeight;
        }

        public float MoraleWeight { get; }

        public float TrainingWeight { get; }

        public float EquipmentWeight { get; }

        public float CommanderWeight { get; }

        public float CohesionWeight { get; }

        public float RankWeight { get; }

        public float CasualtyShockPenaltyWeight { get; }

        public static DoctrineScoreSettings FromConfig(CommanderConfig config)
        {
            if (config == null)
            {
                CommanderConfig d = CommanderConfigDefaults.CreateDefault();
                return new DoctrineScoreSettings(
                    d.MoraleWeight,
                    d.TrainingWeight,
                    d.EquipmentWeight,
                    d.CommanderWeight,
                    d.CohesionWeight,
                    d.RankWeight,
                    d.CasualtyShockPenaltyWeight);
            }

            return new DoctrineScoreSettings(
                config.MoraleWeight,
                config.TrainingWeight,
                config.EquipmentWeight,
                config.CommanderWeight,
                config.CohesionWeight,
                config.RankWeight,
                config.CasualtyShockPenaltyWeight);
        }
    }
}
