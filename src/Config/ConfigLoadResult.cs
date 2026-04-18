namespace Bannerlord.RTSCameraLite.Config
{
    /// <summary>
    /// Outcome of loading <see cref="CommanderConfig"/> from disk (Slice 6), including optional Slice 23 validation metadata.
    /// </summary>
    public sealed class ConfigLoadResult
    {
        public ConfigLoadResult(
            bool loaded,
            bool usedDefaults,
            bool createdDefaultFile,
            string message,
            CommanderConfig config,
            CommanderConfigValidationResult validation = null)
        {
            Loaded = loaded;
            UsedDefaults = usedDefaults;
            CreatedDefaultFile = createdDefaultFile;
            Message = message ?? string.Empty;
            Config = config ?? CommanderConfigDefaults.CreateDefault();
            Validation = validation;
        }

        public bool Loaded { get; }

        public bool UsedDefaults { get; }

        public bool CreatedDefaultFile { get; }

        public string Message { get; }

        public CommanderConfig Config { get; }

        /// <summary>Populated when Slice 23 validation ran for a deserialized config; otherwise null.</summary>
        public CommanderConfigValidationResult Validation { get; }
    }
}
