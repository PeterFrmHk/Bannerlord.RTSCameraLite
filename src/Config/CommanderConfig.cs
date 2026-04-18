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

        // Slice 10 — doctrine scoring weights and scan cadence (data only; no orders).

        public float MoraleWeight { get; set; }

        public float TrainingWeight { get; set; }

        public float EquipmentWeight { get; set; }

        public float CommanderWeight { get; set; }

        public float CohesionWeight { get; set; }

        public float RankWeight { get; set; }

        public float CasualtyShockPenaltyWeight { get; set; }

        public bool EnableDoctrineDebug { get; set; }

        public float DoctrineScanIntervalSeconds { get; set; }

        // Slice 11 — formation eligibility (advisory; no native order blocking).

        public float BasicLineMinimumDiscipline { get; set; }

        public float LooseMinimumDiscipline { get; set; }

        public float ShieldWallMinimumDiscipline { get; set; }

        public float SquareMinimumDiscipline { get; set; }

        public float CircleMinimumDiscipline { get; set; }

        public float AdvancedAdaptiveMinimumDiscipline { get; set; }

        public float MinimumShieldRatioForShieldWall { get; set; }

        public float MinimumPolearmOrShieldRatioForSquare { get; set; }

        public float MinimumMountedRatioForMountedWide { get; set; }

        public float MinimumHorseArcherRatioForHorseArcherLoose { get; set; }

        public bool EnableEligibilityDebug { get; set; }

        // Slice 12 — rally / absorption (planning only; no orders).

        public float CommanderRallyRadius { get; set; }

        public float CommanderAbsorptionRadius { get; set; }

        public float FormationSlotRadius { get; set; }

        public float CohesionBreakRadius { get; set; }

        public float SlotReassignmentCooldownSeconds { get; set; }

        public float RallyScanIntervalSeconds { get; set; }

        public bool EnableRallyAbsorptionDebug { get; set; }

        // Slice 13 — cavalry spacing + charge-release doctrine (planning only; no native orders).

        public float CavalryLateralSpacing { get; set; }

        public float CavalryDepthSpacing { get; set; }

        public float HorseArcherLateralSpacing { get; set; }

        public float HorseArcherDepthSpacing { get; set; }

        public float CavalryReleaseLockDistance { get; set; }

        public float CavalryReformDistanceFromAttackedFormation { get; set; }

        public float CavalryReformCooldownSeconds { get; set; }

        public float CavalryMinimumEnemyDistanceToReform { get; set; }

        public float CavalryImpactEnemyDistance { get; set; }

        public float CavalryImpactSpeedDropThreshold { get; set; }

        public float CavalryImpactAgentRatio { get; set; }

        public bool EnableCavalryDoctrineDebug { get; set; }

        /// <summary>
        /// When true, cavalry reform discipline may proceed without a recognized commander (Slice 13 fallback).
        /// </summary>
        public bool AllowCavalryReformWithoutCommander { get; set; }

        // Slice 15 — command router + formation restriction (validation before native execution).

        public bool EnableCommandRouter { get; set; }

        public bool EnableCommandValidationDebug { get; set; }

        public bool AllowBasicChargeWithoutAdvancedDoctrine { get; set; }

        public bool AllowNoCommanderBasicHold { get; set; }

        public bool AllowNoCommanderBasicFollow { get; set; }

        public bool BlockAdvancedCommandsWithoutCommander { get; set; }

        /// <summary>
        /// When true, command routing may call <c>NativeOrderPrimitiveExecutor</c> after validation.
        /// Default false: validation-only unless explicitly enabled.
        /// </summary>
        public bool EnableNativePrimitiveOrderExecution { get; set; }

        // Slice 16 — native cavalry charge sequence orchestrator (opt-in; requires wired executor).

        /// <summary>When true, <see cref="Commands.CommandType.NativeCavalryChargeSequence"/> may start the orchestrator (still requires native primitives enabled and wired).</summary>
        public bool EnableNativeCavalryChargeSequence { get; set; }

        public bool CavalryUseNativeForwardBeforeCharge { get; set; }

        public bool CavalryUseNativeChargeCommand { get; set; }

        public float CavalryForwardToChargeDistance { get; set; }

        public bool EnableCavalrySequenceDebug { get; set; }

        /// <summary>Minimum seconds between debug validation log lines (Slice 15).</summary>
        public float CommandValidationDebugLogIntervalSeconds { get; set; }
    }
}
