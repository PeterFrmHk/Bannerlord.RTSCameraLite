using TaleWorlds.Library;

namespace Bannerlord.RTSCameraLite.Core
{
    /// <summary>
    /// Safe logging: never throws; InformationManager only after UI is marked ready.
    /// </summary>
    public static class ModLogger
    {
        private static bool _uiReady;
        private static readonly object WarningOnceLock = new object();
        private static readonly System.Collections.Generic.HashSet<string> WarningOnceKeys =
            new System.Collections.Generic.HashSet<string>(System.StringComparer.Ordinal);

        public static void MarkUiReady()
        {
            _uiReady = true;
        }

        /// <summary>
        /// Diagnostic only (never uses InformationManager).
        /// </summary>
        public static void LogDebug(string message)
        {
            try
            {
                string formatted = $"[{ModConstants.ModuleId}] {message}";
                System.Diagnostics.Debug.WriteLine(formatted);
            }
            catch
            {
                // Logging must never crash the game.
            }
        }

        /// <summary>
        /// Emits a single diagnostic line per <paramref name="key"/> for the process (config fallback, path errors, etc.).
        /// </summary>
        public static void LogWarningOnce(string key, string message)
        {
            if (string.IsNullOrEmpty(key))
            {
                key = "default";
            }

            lock (WarningOnceLock)
            {
                if (!WarningOnceKeys.Add(key))
                {
                    return;
                }
            }

            LogDebug($"[WARN:{key}] {message}");
        }

        /// <summary>
        /// Player-visible only when <paramref name="allowUi"/> is true and UI has been marked ready.
        /// </summary>
        public static void PlayerMessage(string message, bool allowUi)
        {
            string formatted = $"[{ModConstants.DisplayName}] {message}";

            try
            {
                System.Diagnostics.Debug.WriteLine(formatted);
            }
            catch
            {
                // Ignore debug sink failures.
            }

            if (!allowUi || !_uiReady)
            {
                return;
            }

            try
            {
                InformationManager.DisplayMessage(new InformationMessage(formatted));
            }
            catch
            {
                // Bannerlord may not have UI messaging ready during early load.
            }
        }

        /// <summary>
        /// InformationManager path (guarded by UI readiness).
        /// </summary>
        public static void Info(string message)
        {
            PlayerMessage(message, allowUi: true);
        }

        /// <summary>
        /// Startup line: safe before UI (debug only); after <see cref="MarkUiReady"/> also mirrors to UI when allowed.
        /// </summary>
        public static void SafeStartupLog(string message)
        {
            PlayerMessage(message, allowUi: true);
        }
    }
}
