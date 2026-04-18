using Bannerlord.RTSCameraLite.Config;

namespace Bannerlord.RTSCameraLite.Adapters
{
    /// <summary>
    /// Version-sensitive coordination for RTS Backspace vs vanilla mission-order UI.
    /// Slice 0 / order scan: no verified public API to block native Backspace consumption from managed code without Harmony (explicitly avoided).
    /// All future engine-facing suppression must stay inside this type.
    /// </summary>
    public sealed class BackspaceConflictGuard
    {
        private const string UnsupportedBody =
            "Slice 0: no verified managed input-block API for native MissionOrder UI vs Backspace (see docs/research/implementation-decision-slice0.md §7, base-game-order-scan.md §8).";

        private CommanderConfig _config = CommanderConfigDefaults.CreateDefault();
        private bool _commanderModeActive;
        private BackspaceConflictResult _lastEvaluation = BackspaceConflictResult.Inactive();

        public void ApplyConfig(CommanderConfig config)
        {
            _config = config ?? CommanderConfigDefaults.CreateDefault();
            Recompute();
        }

        public void EnterCommanderMode()
        {
            _commanderModeActive = true;
            Recompute();
        }

        public void ExitCommanderMode()
        {
            _commanderModeActive = false;
            Recompute();
        }

        /// <summary>
        /// When <c>true</c>, callers could skip forwarding Backspace to native systems. Slice 7: always <c>false</c> (safe).
        /// </summary>
        public bool ShouldSuppressNativeBackspace()
        {
            return false;
        }

        public void Tick()
        {
            Recompute();
        }

        public void Cleanup()
        {
            _commanderModeActive = false;
            _lastEvaluation = BackspaceConflictResult.Inactive("cleanup");
        }

        public BackspaceConflictResult LastEvaluation => _lastEvaluation;

        private void Recompute()
        {
            if (!_commanderModeActive)
            {
                _lastEvaluation = BackspaceConflictResult.Inactive();
                return;
            }

            if (!_config.OverrideNativeBackspaceOrders)
            {
                _lastEvaluation = BackspaceConflictResult.Inactive("OverrideNativeBackspaceOrders is false");
                return;
            }

            _lastEvaluation = BackspaceConflictResult.Unsupported(UnsupportedBody);
        }
    }
}
