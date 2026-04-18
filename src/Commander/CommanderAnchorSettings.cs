using Bannerlord.RTSCameraLite.Config;

namespace Bannerlord.RTSCameraLite.Commander
{
    /// <summary>
    /// Tunables for commander anchor placement (Slice 9).
    /// </summary>
    public sealed class CommanderAnchorSettings
    {
        public float DefaultCommanderBackOffset { get; set; }

        public float ShieldWallCommanderBackOffset { get; set; }

        public float ArcherCommanderBackOffset { get; set; }

        public float CavalryCommanderBackOffset { get; set; }

        public float SkirmisherCommanderBackOffset { get; set; }

        public float AnchorAllowedRadius { get; set; }

        public bool EnableCommanderAnchorDebug { get; set; }

        public static CommanderAnchorSettings FromConfig(CommanderConfig config)
        {
            CommanderConfig c = config ?? CommanderConfigDefaults.CreateDefault();
            return new CommanderAnchorSettings
            {
                DefaultCommanderBackOffset = c.DefaultCommanderBackOffset,
                ShieldWallCommanderBackOffset = c.ShieldWallCommanderBackOffset,
                ArcherCommanderBackOffset = c.ArcherCommanderBackOffset,
                CavalryCommanderBackOffset = c.CavalryCommanderBackOffset,
                SkirmisherCommanderBackOffset = c.SkirmisherCommanderBackOffset,
                AnchorAllowedRadius = c.AnchorAllowedRadius,
                EnableCommanderAnchorDebug = c.EnableCommanderAnchorDebug
            };
        }
    }
}
