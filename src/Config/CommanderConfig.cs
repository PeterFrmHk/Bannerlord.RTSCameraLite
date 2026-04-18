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

        // Slice 8 — commander presence policy (detection only; no formation restrictions yet).

        public bool RequireHeroCommanderForAdvancedFormations { get; set; }

        public bool AllowCaptainCommander { get; set; }

        public bool AllowSergeantFallback { get; set; }

        public bool AllowHighestTierFallback { get; set; }

        public bool NoCommanderAllowsBasicMobOrders { get; set; }

        public float MinimumCommandAuthorityScore { get; set; }

        // Slice 9 — commander anchor (compute only; no movement).

        public float DefaultCommanderBackOffset { get; set; }

        public float ShieldWallCommanderBackOffset { get; set; }

        public float ArcherCommanderBackOffset { get; set; }

        public float CavalryCommanderBackOffset { get; set; }

        public float SkirmisherCommanderBackOffset { get; set; }

        public float AnchorAllowedRadius { get; set; }

        public bool EnableCommanderAnchorDebug { get; set; }
    }
}
