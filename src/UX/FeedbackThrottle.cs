using System;
using System.Collections.Generic;

namespace Bannerlord.RTSCameraLite.UX
{
    /// <summary>
    /// Cooldowns for in-mission feedback so InformationManager is not spammed every frame.
    /// </summary>
    internal sealed class FeedbackThrottle
    {
        private readonly Dictionary<string, DateTime> _lastShownUtc = new Dictionary<string, DateTime>();

        /// <summary>
        /// Returns true if the caller may show feedback for this key. When <paramref name="forceImmediate"/> is true,
        /// the key is refreshed but always returns true (important one-shots).
        /// </summary>
        public bool TryAllow(string key, double cooldownSeconds, bool forceImmediate)
        {
            if (string.IsNullOrEmpty(key))
            {
                key = "default";
            }

            DateTime now = DateTime.UtcNow;
            if (forceImmediate)
            {
                _lastShownUtc[key] = now;
                return true;
            }

            if (_lastShownUtc.TryGetValue(key, out DateTime last))
            {
                if ((now - last).TotalSeconds < cooldownSeconds)
                {
                    return false;
                }
            }

            _lastShownUtc[key] = now;
            return true;
        }

        public void ClearKey(string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                return;
            }

            _lastShownUtc.Remove(key);
        }

        public void ResetAll()
        {
            _lastShownUtc.Clear();
        }
    }
}
