using System.Collections.Generic;

namespace Bannerlord.RTSCameraLite.Config
{
    /// <summary>
    /// Outcome of validating and sanitizing a <see cref="CommanderConfig"/> instance (Slice 23).
    /// </summary>
    public sealed class CommanderConfigValidationResult
    {
        public CommanderConfigValidationResult(
            bool isValid,
            bool usedFallbacks,
            bool requiresRewrite,
            List<string> warnings,
            List<string> errors,
            CommanderConfig sanitizedConfig)
        {
            IsValid = isValid;
            UsedFallbacks = usedFallbacks;
            RequiresRewrite = requiresRewrite;
            Warnings = warnings ?? new List<string>();
            Errors = errors ?? new List<string>();
            SanitizedConfig = sanitizedConfig ?? CommanderConfigDefaults.CreateDefault();
        }

        public bool IsValid { get; }

        public bool UsedFallbacks { get; }

        /// <summary>True when the on-disk file should be rewritten to match the sanitized model.</summary>
        public bool RequiresRewrite { get; }

        public List<string> Warnings { get; }

        public List<string> Errors { get; }

        public CommanderConfig SanitizedConfig { get; }
    }
}
