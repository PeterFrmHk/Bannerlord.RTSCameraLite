namespace Bannerlord.RTSCameraLite.UX
{
    /// <summary>Resolves marker TTL by marker type (Slice 19).</summary>
    public static class MarkerLifetime
    {
        public static float Resolve(CommandMarkerType type, CommandMarkerSettings settings)
        {
            if (settings == null)
            {
                return 2.5f;
            }

            switch (type)
            {
                case CommandMarkerType.ChargeTarget:
                    return System.Math.Max(0.1f, settings.ChargeMarkerLifetimeSeconds);
                case CommandMarkerType.ReformPoint:
                    return System.Math.Max(0.1f, settings.ReformMarkerLifetimeSeconds);
                default:
                    return System.Math.Max(0.1f, settings.DefaultMarkerLifetimeSeconds);
            }
        }
    }
}
