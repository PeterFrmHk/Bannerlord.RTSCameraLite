using System.Collections.Generic;

namespace Bannerlord.RTSCameraLite.Doctrine
{
    /// <summary>
    /// Outcome of <see cref="FormationRestrictionService.Evaluate"/> (Slice 15).
    /// </summary>
    public sealed class RestrictionDecision
    {
        private RestrictionDecision(
            bool allowed,
            bool blocked,
            string reason,
            List<AllowedFormationType> requiredFormationTypes)
        {
            Allowed = allowed;
            Blocked = blocked;
            Reason = reason ?? string.Empty;
            RequiredFormationTypes = requiredFormationTypes ?? new List<AllowedFormationType>();
        }

        public bool Allowed { get; }

        public bool Blocked { get; }

        public string Reason { get; }

        public List<AllowedFormationType> RequiredFormationTypes { get; }

        public static RestrictionDecision Allow(string reason = "", List<AllowedFormationType> requiredFormationTypes = null)
        {
            return new RestrictionDecision(true, false, reason, requiredFormationTypes);
        }

        public static RestrictionDecision Deny(string reason, List<AllowedFormationType> requiredFormationTypes = null)
        {
            return new RestrictionDecision(false, false, reason, requiredFormationTypes);
        }

        public static RestrictionDecision Block(string reason, List<AllowedFormationType> requiredFormationTypes = null)
        {
            return new RestrictionDecision(false, true, reason, requiredFormationTypes);
        }
    }
}
