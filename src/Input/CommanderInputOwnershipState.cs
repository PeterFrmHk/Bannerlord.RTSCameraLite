namespace Bannerlord.RTSCameraLite.Input
{
    /// <summary>
    /// Advisory snapshot of which inputs the mod intends to coordinate while Commander Mode is active (Slice 7).
    /// Flags describe <b>intent</b>; enforcement depends on engine-safe hooks (see <see cref="CommanderNativeInputGuard"/>).
    /// </summary>
    public readonly struct CommanderInputOwnershipState
    {
        public CommanderInputOwnershipState(
            bool commanderModeOwnsActivationKey,
            bool nativeOrderMenuSuppressionRequested,
            bool nativeMovementSuppressionRequested,
            bool nativeCombatSuppressionRequested,
            string currentReason)
        {
            CommanderModeOwnsActivationKey = commanderModeOwnsActivationKey;
            NativeOrderMenuSuppressionRequested = nativeOrderMenuSuppressionRequested;
            NativeMovementSuppressionRequested = nativeMovementSuppressionRequested;
            NativeCombatSuppressionRequested = nativeCombatSuppressionRequested;
            CurrentReason = currentReason ?? string.Empty;
        }

        /// <summary>True when commander mode is on and the input-ownership guard is enabled.</summary>
        public bool CommanderModeOwnsActivationKey { get; }

        /// <summary>True when config requests native order menu coordination (not enforceable without a verified API).</summary>
        public bool NativeOrderMenuSuppressionRequested { get; }

        /// <summary>Reserved for future movement suppression (Slice 0: no safe path — remains false effect).</summary>
        public bool NativeMovementSuppressionRequested { get; }

        /// <summary>Reserved for future combat suppression (Slice 0: no safe path — remains false effect).</summary>
        public bool NativeCombatSuppressionRequested { get; }

        public string CurrentReason { get; }

        public static CommanderInputOwnershipState Inactive(string reason = "")
        {
            return new CommanderInputOwnershipState(false, false, false, false, string.IsNullOrEmpty(reason) ? "inactive" : reason);
        }
    }
}
