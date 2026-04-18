namespace Bannerlord.RTSCameraLite.Config
{
    /// <summary>
    /// File-backed RTS Commander Mode profile (Slice 6). Serialized as JSON from the module config folder.
    /// </summary>
    public sealed class CommanderConfig
    {
        public bool StartBattlesInCommanderMode { get; set; }

        public string ModeActivationKey { get; set; } = string.Empty;

        public bool OverrideNativeBackspaceOrders { get; set; }

        /// <summary>
        /// When commander mode is off, native order UI may use the same physical keys as the profile default.
        /// Slice 7: policy only — managed suppression of native order menu is not available (see research).
        /// </summary>
        public bool AllowNativeOrdersWhenCommanderModeDisabled { get; set; }

        /// <summary>When true, exposes advisory input ownership state while commander mode is on.</summary>
        public bool EnableInputOwnershipGuard { get; set; }

        /// <summary>Reserved; no safe suppression path in Slice 7 — must stay false until research approves an engine hook.</summary>
        public bool SuppressNativeMovementInCommanderMode { get; set; }

        /// <summary>Reserved; no safe suppression path in Slice 7 — must stay false until research approves an engine hook.</summary>
        public bool SuppressNativeCombatInCommanderMode { get; set; }

        public bool EnableDebugFallbackToggle { get; set; }

        public string DebugFallbackToggleKey { get; set; } = string.Empty;

        public string MoveForwardKey { get; set; } = string.Empty;

        public string MoveBackKey { get; set; } = string.Empty;

        public string MoveLeftKey { get; set; } = string.Empty;

        public string MoveRightKey { get; set; } = string.Empty;

        public string RotateLeftKey { get; set; } = string.Empty;

        public string RotateRightKey { get; set; } = string.Empty;

        public string FastMoveKey { get; set; } = string.Empty;

        public string ZoomInKey { get; set; } = string.Empty;

        public string ZoomOutKey { get; set; } = string.Empty;

        public float MoveSpeed { get; set; }

        public float FastMoveMultiplier { get; set; }

        public float RotationSpeedDegrees { get; set; }

        public float ZoomSpeed { get; set; }

        public float DefaultHeight { get; set; }

        public float MinHeight { get; set; }

        public float MaxHeight { get; set; }

        public float DefaultPitch { get; set; }
    }
}
