using System;
using Bannerlord.RTSCameraLite.Equipment;

namespace Bannerlord.RTSCameraLite.Doctrine
{
    /// <summary>
    /// Doctrine aggregates for morale, training, equipment, cohesion, shock, and discipline synthesis (Slice 10 — data only).
    /// </summary>
    public sealed class FormationDoctrineProfile
    {
        public FormationDoctrineProfile(
            float moraleScore,
            float trainingScore,
            float equipmentScore,
            float commanderScore,
            float cohesionScore,
            float casualtyShock,
            float rankQualityScore,
            float formationDisciplineScore,
            FormationCompositionProfile composition,
            string reason,
            bool isCertain)
        {
            MoraleScore = Clamp01(moraleScore);
            TrainingScore = Clamp01(trainingScore);
            EquipmentScore = Clamp01(equipmentScore);
            CommanderScore = Clamp01(commanderScore);
            CohesionScore = Clamp01(cohesionScore);
            CasualtyShock = Clamp01(casualtyShock);
            RankQualityScore = Clamp01(rankQualityScore);
            FormationDisciplineScore = Clamp01(formationDisciplineScore);
            Composition = composition ?? FormationCompositionProfile.Empty("missing composition");
            Reason = reason ?? string.Empty;
            IsCertain = isCertain;
        }

        public float MoraleScore { get; }

        public float TrainingScore { get; }

        public float EquipmentScore { get; }

        public float CommanderScore { get; }

        public float CohesionScore { get; }

        /// <summary>0..1 damage shock proxy (higher = more depleted / shocked).</summary>
        public float CasualtyShock { get; }

        public float RankQualityScore { get; }

        public float FormationDisciplineScore { get; }

        public FormationCompositionProfile Composition { get; }

        public string Reason { get; }

        public bool IsCertain { get; }

        private static float Clamp01(float v)
        {
            if (float.IsNaN(v) || float.IsInfinity(v))
            {
                return 0f;
            }

            return (float)Math.Max(0d, Math.Min(1d, v));
        }
    }
}
