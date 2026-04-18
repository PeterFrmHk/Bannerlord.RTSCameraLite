using Bannerlord.RTSCameraLite.Config;

namespace Bannerlord.RTSCameraLite.UX
{
    /// <summary>Marker UX policy loaded from <see cref="CommanderConfig"/> (Slice 19).</summary>
    public sealed class CommandMarkerSettings
    {
        public bool EnableCommandMarkers { get; set; }

        public bool EnableFallbackTextMarkers { get; set; }

        public float DefaultMarkerLifetimeSeconds { get; set; }

        public float ChargeMarkerLifetimeSeconds { get; set; }

        public float ReformMarkerLifetimeSeconds { get; set; }

        public float MarkerRefreshThrottleSeconds { get; set; }

        public static CommandMarkerSettings FromConfig(CommanderConfig config)
        {
            CommanderConfig c = config ?? CommanderConfigDefaults.CreateDefault();
            return new CommandMarkerSettings
            {
                EnableCommandMarkers = c.EnableCommandMarkers,
                EnableFallbackTextMarkers = c.EnableFallbackTextMarkers,
                DefaultMarkerLifetimeSeconds = c.DefaultMarkerLifetimeSeconds,
                ChargeMarkerLifetimeSeconds = c.ChargeMarkerLifetimeSeconds,
                ReformMarkerLifetimeSeconds = c.ReformMarkerLifetimeSeconds,
                MarkerRefreshThrottleSeconds = c.MarkerRefreshThrottleSeconds
            };
        }
    }
}
