namespace Bannerlord.RTSCameraLite.Doctrine
{
    /// <summary>Outcome of a doctrine score pass (Slice 10).</summary>
    public sealed class DoctrineScoreResult
    {
        private DoctrineScoreResult(
            bool computationSucceeded,
            FormationDoctrineProfile profile,
            string message,
            bool usedFallbacks,
            bool uncertainOutcome)
        {
            ComputationSucceeded = computationSucceeded;
            Profile = profile;
            Message = message ?? string.Empty;
            UsedFallbacks = usedFallbacks;
            UncertainOutcome = uncertainOutcome;
        }

        /// <summary>True when a profile was produced (including uncertain estimates).</summary>
        public bool ComputationSucceeded { get; }

        public FormationDoctrineProfile Profile { get; }

        public string Message { get; }

        public bool UsedFallbacks { get; }

        /// <summary>True when the profile should be treated as low-confidence advisory data.</summary>
        public bool UncertainOutcome { get; }

        public static DoctrineScoreResult Success(FormationDoctrineProfile profile, string message = "", bool usedFallbacks = false)
        {
            return new DoctrineScoreResult(true, profile, message, usedFallbacks, false);
        }

        public static DoctrineScoreResult Failure(string message)
        {
            return new DoctrineScoreResult(false, null, message, true, false);
        }

        public static DoctrineScoreResult Uncertain(FormationDoctrineProfile profile, string message, bool usedFallbacks = true)
        {
            return new DoctrineScoreResult(true, profile, message, usedFallbacks, true);
        }
    }
}
