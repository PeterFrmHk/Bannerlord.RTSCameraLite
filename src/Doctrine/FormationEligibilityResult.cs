using System.Collections.Generic;

namespace Bannerlord.RTSCameraLite.Doctrine
{
    /// <summary>
    /// Allow/deny sets for formation behaviors (Slice 11).
    /// </summary>
    public sealed class FormationEligibilityResult
    {
        private FormationEligibilityResult(
            bool success,
            bool hasAdvancedFormationAccess,
            List<AllowedFormationType> allowedFormationTypes,
            List<AllowedFormationType> deniedFormationTypes,
            string reason,
            bool isCertain)
        {
            Success = success;
            HasAdvancedFormationAccess = hasAdvancedFormationAccess;
            AllowedFormationTypes = allowedFormationTypes ?? new List<AllowedFormationType>();
            DeniedFormationTypes = deniedFormationTypes ?? new List<AllowedFormationType>();
            Reason = reason ?? string.Empty;
            IsCertain = isCertain;
        }

        public bool Success { get; }

        public bool HasAdvancedFormationAccess { get; }

        public List<AllowedFormationType> AllowedFormationTypes { get; }

        public List<AllowedFormationType> DeniedFormationTypes { get; }

        public string Reason { get; }

        public bool IsCertain { get; }

        /// <summary>Factory for a completed evaluation (C# cannot name this <c>Success</c> alongside the <see cref="Success"/> property).</summary>
        public static FormationEligibilityResult CreateSuccess(
            bool hasAdvancedFormationAccess,
            List<AllowedFormationType> allowedFormationTypes,
            List<AllowedFormationType> deniedFormationTypes,
            bool isCertain,
            string reason)
        {
            return new FormationEligibilityResult(
                true,
                hasAdvancedFormationAccess,
                allowedFormationTypes,
                deniedFormationTypes,
                reason,
                isCertain);
        }

        public static FormationEligibilityResult Failure(string reason)
        {
            return new FormationEligibilityResult(
                false,
                false,
                new List<AllowedFormationType>(),
                new List<AllowedFormationType>(),
                reason,
                isCertain: true);
        }

        public static FormationEligibilityResult Uncertain(
            string reason,
            List<AllowedFormationType> allowedFormationTypes,
            List<AllowedFormationType> deniedFormationTypes,
            bool hasAdvancedFormationAccess)
        {
            return new FormationEligibilityResult(
                true,
                hasAdvancedFormationAccess,
                allowedFormationTypes,
                deniedFormationTypes,
                reason,
                isCertain: false);
        }
    }
}
