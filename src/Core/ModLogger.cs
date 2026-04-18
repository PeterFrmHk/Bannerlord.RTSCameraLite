using TaleWorlds.Library;

namespace Bannerlord.RTSCameraLite.Core
{
    internal static class ModLogger
    {
        private static bool _uiReady;

        public static void MarkUiReady()
        {
            _uiReady = true;
        }

        /// <summary>
        /// Diagnostic text only (never raises to InformationManager).
        /// </summary>
        public static void LogDebug(string message)
        {
            try
            {
                string formatted = $"[{ModConstants.DisplayName}] {message}";
                System.Diagnostics.Debug.WriteLine(formatted);
            }
            catch
            {
                // Swallow: logging must never crash the game.
            }
        }

        /// <summary>
        /// Player-visible when <paramref name="allowUi"/> and UI are ready; always mirrors to debug output when possible.
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

            if (!allowUi)
            {
                return;
            }

            try
            {
                if (_uiReady)
                {
                    InformationManager.DisplayMessage(new InformationMessage(formatted));
                }
            }
            catch
            {
                // Bannerlord may not have UI messaging ready during early load.
            }
        }

        /// <summary>
        /// UI path: only uses InformationManager after the initial module screen is available.
        /// </summary>
        public static void Info(string message)
        {
            PlayerMessage(message, allowUi: true);
        }

        public static void SafeStartupLog(string message)
        {
            PlayerMessage(message, allowUi: true);
        }
    }
}
