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
            string reason,
            bool isMountedLayout = false,
            bool isHorseArcherLayout = false,
            bool positionLockAllowed = false,
            bool releaseLockAfterCloseContact = false,
            float reformDistance = 30f,
            string mountedDoctrineReason = "")
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
            IsMountedLayout = isMountedLayout;
            IsHorseArcherLayout = isHorseArcherLayout;
            PositionLockAllowed = positionLockAllowed;
            ReleaseLockAfterCloseContact = releaseLockAfterCloseContact;
            ReformDistance = reformDistance;
            MountedDoctrineReason = mountedDoctrineReason ?? string.Empty;
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

        public bool IsMountedLayout { get; }

        public bool IsHorseArcherLayout { get; }

        public bool PositionLockAllowed { get; }

        public bool ReleaseLockAfterCloseContact { get; }

        public float ReformDistance { get; }

        public string MountedDoctrineReason { get; }
    }
}
