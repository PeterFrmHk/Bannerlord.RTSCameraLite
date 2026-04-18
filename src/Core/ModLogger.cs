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
        /// UI path: only uses InformationManager after the initial module screen is available.
        /// </summary>
        public static void Info(string message)
        {
            string formatted = $"[{ModConstants.DisplayName}] {message}";

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

            System.Diagnostics.Debug.WriteLine(formatted);
        }

        public static void SafeStartupLog(string message)
        {
            string formatted = $"[{ModConstants.DisplayName}] {message}";

            try
            {
                if (_uiReady)
                {
                    InformationManager.DisplayMessage(new InformationMessage(formatted));
                }
            }
            catch
            {
                // Intentionally swallowed for Slice 1 stability.
            }

            System.Diagnostics.Debug.WriteLine(formatted);
        }
    }
}
