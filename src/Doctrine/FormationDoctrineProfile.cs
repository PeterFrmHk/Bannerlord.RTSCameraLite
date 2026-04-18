namespace Bannerlord.RTSCameraLite.Doctrine
{
    /// <summary>
    /// Doctrine aggregates combining morale/training/equipment/rank proxies (Slice 11).
    /// </summary>
    public sealed class FormationDoctrineProfile
    {
        public FormationDoctrineProfile(
            float disciplineScore,
            float morale01,
            float training01,
            float equipment01,
            float rank01,
            FormationCompositionProfile composition,
            bool isCertain,
            string reason)
        {
            DisciplineScore = disciplineScore;
            Morale01 = morale01;
            Training01 = training01;
            Equipment01 = equipment01;
            Rank01 = rank01;
            Composition = composition;
            IsCertain = isCertain;
            Reason = reason ?? string.Empty;
        }

        /// <summary>Primary gate for line discipline (0..1).</summary>
        public float DisciplineScore { get; }

        public float Morale01 { get; }

        public float Training01 { get; }

        public float Equipment01 { get; }

        public float Rank01 { get; }

        public FormationCompositionProfile Composition { get; }

        public bool IsCertain { get; }

        public string Reason { get; }
    }
}
