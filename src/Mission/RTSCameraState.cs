namespace Bannerlord.RTSCameraLite.Mission
{
    internal sealed class RTSCameraState
    {
        public bool Enabled { get; private set; }
        public int ToggleCount { get; private set; }
        public string LastToggleReason { get; private set; } = "Not toggled";

        public void Toggle(string reason)
        {
            Enabled = !Enabled;
            ToggleCount++;
            LastToggleReason = reason;
        }

        /// <summary>
        /// Clears RTS without incrementing toggle count (e.g. battle ended while RTS was on).
        /// </summary>
        public void ForceDisabled(string reason)
        {
            if (!Enabled)
            {
                return;
            }

            Enabled = false;
            LastToggleReason = reason;
        }
    }
}
