namespace Bannerlord.RTSCameraLite.Doctrine
{
    /// <summary>
    /// Planned row/rank spacing parameters for layout after rally absorption (Slice 12).
    /// </summary>
    public sealed class RowRankSpacingPlan
    {
        public RowRankSpacingPlan(
            int rowCount,
            int rankDepth,
            float lateralSpacing,
            float depthSpacing,
            bool useLooseSpacing,
            bool useTightSpacing,
            string frontRowRule,
            string secondRowRule,
            string rearRankRule,
            float confidence,
            string reason)
        {
            RowCount = rowCount;
            RankDepth = rankDepth;
            LateralSpacing = lateralSpacing;
            DepthSpacing = depthSpacing;
            UseLooseSpacing = useLooseSpacing;
            UseTightSpacing = useTightSpacing;
            FrontRowRule = frontRowRule ?? string.Empty;
            SecondRowRule = secondRowRule ?? string.Empty;
            RearRankRule = rearRankRule ?? string.Empty;
            Confidence = confidence;
            Reason = reason ?? string.Empty;
        }

        public int RowCount { get; }

        public int RankDepth { get; }

        public float LateralSpacing { get; }

        public float DepthSpacing { get; }

        public bool UseLooseSpacing { get; }

        public bool UseTightSpacing { get; }

        public string FrontRowRule { get; }

        public string SecondRowRule { get; }

        public string RearRankRule { get; }

        public float Confidence { get; }

        public string Reason { get; }
    }
}
