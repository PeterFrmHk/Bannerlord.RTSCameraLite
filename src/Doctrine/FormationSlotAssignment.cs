namespace Bannerlord.RTSCameraLite.Doctrine
{
    /// <summary>
    /// Planned row/rank/file slot for a troop after absorption (Slice 12 — data only).
    /// </summary>
    public sealed class FormationSlotAssignment
    {
        public FormationSlotAssignment(
            int row,
            int rank,
            int fileIndex,
            bool isLeftFlank,
            bool isRightFlank,
            string ruleTag)
        {
            Row = row;
            Rank = rank;
            FileIndex = fileIndex;
            IsLeftFlank = isLeftFlank;
            IsRightFlank = isRightFlank;
            RuleTag = ruleTag ?? string.Empty;
        }

        public int Row { get; }

        public int Rank { get; }

        public int FileIndex { get; }

        public bool IsLeftFlank { get; }

        public bool IsRightFlank { get; }

        public string RuleTag { get; }
    }
}
