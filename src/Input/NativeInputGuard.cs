namespace Bannerlord.RTSCameraLite.Input
{
    /// <summary>
    /// Central place for RTS vs native input ownership and (future) suppression hooks.
    /// Slice 6: no Harmony; TaleWorlds does not expose a stable public API here to block
    /// agent movement while still polling the same keys for RTS, so suppression is advisory only.
    /// </summary>
    internal sealed class NativeInputGuard
    {
        private readonly InputOwnershipState _ownership = new InputOwnershipState();
        private bool _rtsMode;

        public InputOwnershipState Ownership => _ownership;

        public bool IsRtsModeActive => _rtsMode;

        public void EnterRtsMode(string reason = "RTS camera")
        {
            if (_rtsMode)
            {
                return;
            }

            _rtsMode = true;
            _ownership.RtsModeOwnsCameraInput = true;
            _ownership.NativeMovementSuppressionRequested = true;
            _ownership.NativeCombatSuppressionRequested = true;
            _ownership.CurrentReason = reason;
        }

        public void ExitRtsMode(string reason = "RTS camera off")
        {
            if (!_rtsMode)
            {
                return;
            }

            _rtsMode = false;
            _ownership.RtsModeOwnsCameraInput = false;
            _ownership.NativeMovementSuppressionRequested = false;
            _ownership.NativeCombatSuppressionRequested = false;
            _ownership.CurrentReason = reason;
        }

        /// <summary>
        /// Per-frame hook for future public suppression APIs or debounced cleanup.
        /// </summary>
        public void Tick(float dt)
        {
            _ = dt;
            // Intentionally empty: no verified public Bannerlord API to suppress native agent
            // movement/combat input while IInputContext still reports WASD for RTS reads.
        }
    }
}
