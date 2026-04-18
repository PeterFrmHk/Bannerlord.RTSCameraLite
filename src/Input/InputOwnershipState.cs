namespace Bannerlord.RTSCameraLite.Input
{
    /// <summary>
    /// Tracks who should read RTS camera movement keys and whether native suppression is requested.
    /// </summary>
    internal sealed class InputOwnershipState
    {
        public bool RtsModeOwnsCameraInput { get; internal set; }

        public bool NativeMovementSuppressionRequested { get; internal set; }

        public bool NativeCombatSuppressionRequested { get; internal set; }

        public string CurrentReason { get; internal set; } = string.Empty;
    }
}
