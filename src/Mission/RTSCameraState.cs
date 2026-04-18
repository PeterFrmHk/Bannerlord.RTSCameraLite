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
    }
}
