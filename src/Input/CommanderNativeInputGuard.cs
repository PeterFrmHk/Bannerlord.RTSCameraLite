using Bannerlord.RTSCameraLite.Config;

namespace Bannerlord.RTSCameraLite.Input
{
    /// <summary>
    /// Central place for Commander Mode input ownership <b>policy</b> and future engine hooks (Slice 7).
    /// First implementation is advisory only: Slice 0 did not identify a safe managed API to suppress native Backspace / order UI.
    /// </summary>
    public sealed class CommanderNativeInputGuard
    {
        private CommanderConfig _config = CommanderConfigDefaults.CreateDefault();
        private bool _commanderModeActive;
        private CommanderInputOwnershipState _ownership = CommanderInputOwnershipState.Inactive();

        public CommanderInputOwnershipState Ownership => _ownership;

        public void ApplyConfig(CommanderConfig config)
        {
            _config = config ?? CommanderConfigDefaults.CreateDefault();
            RefreshOwnership();
        }

        public void EnterCommanderMode()
        {
            _commanderModeActive = true;
            RefreshOwnership();
        }

        public void ExitCommanderMode()
        {
            _commanderModeActive = false;
            RefreshOwnership();
        }

        public void Tick()
        {
            RefreshOwnership();
        }

        public void Cleanup()
        {
            _commanderModeActive = false;
            _ownership = CommanderInputOwnershipState.Inactive("cleanup");
        }

        /// <summary>
        /// When true, the mod treats the configured activation key as logically owned for RTS commander toggling while commander mode is active.
        /// </summary>
        public bool ShouldOwnActivationKey()
        {
            return _ownership.CommanderModeOwnsActivationKey;
        }

        /// <summary>
        /// When true, callers would suppress native order menu input. Slice 7: always false (no verified suppression path).
        /// </summary>
        public bool ShouldSuppressNativeOrderMenu()
        {
            return false;
        }

        private void RefreshOwnership()
        {
            if (!_commanderModeActive || !_config.EnableInputOwnershipGuard)
            {
                _ownership = CommanderInputOwnershipState.Inactive(_commanderModeActive ? "ownership_guard_disabled" : "commander_mode_off");
                return;
            }

            bool orderMenu = _config.OverrideNativeBackspaceOrders;
            bool movement = _config.SuppressNativeMovementInCommanderMode;
            bool combat = _config.SuppressNativeCombatInCommanderMode;
            _ownership = new CommanderInputOwnershipState(
                commanderModeOwnsActivationKey: true,
                nativeOrderMenuSuppressionRequested: orderMenu,
                nativeMovementSuppressionRequested: movement,
                nativeCombatSuppressionRequested: combat,
                currentReason: "commander_mode_active");
        }
    }
}
