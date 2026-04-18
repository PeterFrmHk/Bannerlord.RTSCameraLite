using System;
using TaleWorlds.InputSystem;

namespace Bannerlord.RTSCameraLite.Input
{
    internal static class CommanderInputKeyParser
    {
        /// <summary>
        /// Resolves a display/config key name to <see cref="InputKey"/>; returns <paramref name="fallback"/> when invalid.
        /// </summary>
        public static InputKey ParseOrFallback(string name, InputKey fallback)
        {
            if (TryParse(name, out InputKey key))
            {
                return key;
            }

            return fallback;
        }

        public static bool TryParse(string name, out InputKey key)
        {
            key = default;
            if (string.IsNullOrWhiteSpace(name))
            {
                return false;
            }

            string normalized = name.Trim().Replace(" ", string.Empty);
            if (string.Equals(normalized, "Backspace", StringComparison.OrdinalIgnoreCase))
            {
                normalized = "BackSpace";
            }

            if (!Enum.TryParse(normalized, ignoreCase: true, out key))
            {
                return false;
            }

            if (!Enum.IsDefined(typeof(InputKey), key))
            {
                return false;
            }

            return true;
        }
    }
}
