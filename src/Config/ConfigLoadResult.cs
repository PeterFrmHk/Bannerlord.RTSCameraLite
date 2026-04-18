namespace Bannerlord.RTSCameraLite.Config
{
    /// <summary>
    /// Outcome of loading <see cref="CommanderConfig"/> from disk (Slice 6).
    /// </summary>
    public sealed class ConfigLoadResult
    {
        public ConfigLoadResult(
            bool loaded,
            bool usedDefaults,
            bool createdDefaultFile,
            string message,
            CommanderConfig config)
        {
            Loaded = loaded;
            UsedDefaults = usedDefaults;
            CreatedDefaultFile = createdDefaultFile;
            Message = message ?? string.Empty;
            Config = config ?? CommanderConfigDefaults.CreateDefault();
        }

        public bool Loaded { get; }

        public bool UsedDefaults { get; }

        public bool CreatedDefaultFile { get; }

        public string Message { get; }

        public CommanderConfig Config { get; }
    }
}
