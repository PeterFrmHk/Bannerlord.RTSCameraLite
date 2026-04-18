namespace Bannerlord.RTSCameraLite.Adapters
{
    /// <summary>
    /// Future hook for coordinating RTS Backspace with vanilla mission-order hotkeys.
    /// Slice 3: TaleWorlds does not expose a supported, verified API here to block native Backspace consumption from managed code.
    /// <see cref="ShouldSuppressNativeBackspace"/> therefore always returns <c>false</c> (safe no-op).
    /// </summary>
    public sealed class BackspaceConflictGuard
    {
        private bool _commanderModeActive;

        public void EnterCommanderMode()
        {
            _commanderModeActive = true;
        }

        public void ExitCommanderMode()
        {
            _commanderModeActive = false;
        }

        /// <summary>
        /// When <c>true</c>, callers could attempt to skip forwarding Backspace to native systems. Not implemented in Slice 3.
        /// </summary>
        public bool ShouldSuppressNativeBackspace()
        {
            _ = _commanderModeActive;
            return false;
        }

        public void Tick()
        {
            // Reserved for future input-stack coordination once engine contracts are verified.
        }

        public BackspaceConflictResult LastEvaluation =>
            new BackspaceConflictResult(false, "Slice 3: suppression not wired (no verified native input block API).");
    }
}
