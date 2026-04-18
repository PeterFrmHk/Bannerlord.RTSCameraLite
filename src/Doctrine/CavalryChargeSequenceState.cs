using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace Bannerlord.RTSCameraLite.Doctrine
{
    /// <summary>
    /// Per-formation cavalry doctrine snapshot for spacing, lock release, and reform gating (Slice 13),
    /// extended with native-sequence execution fields (Slice 16).
    /// </summary>
    public sealed class CavalryChargeSequenceState
    {
        private readonly Formation _sourceFormation;

        public CavalryChargeSequenceState(
            Formation sourceFormation,
            Formation targetFormation,
            CavalryChargeState currentState,
            Vec3? targetPosition,
            Vec3? impactPosition,
            bool positionLockReleased,
            bool reformAllowed,
            bool commanderValid,
            float distanceToTargetFormation,
            float distanceFromImpactPosition,
            float timeSinceLockRelease,
            string reason,
            bool nativeForwardIssued = false,
            bool nativeChargeIssued = false,
            float sequenceTimeSeconds = 0f,
            bool aborted = false,
            string abortReason = null,
            Vec3? lastKnownTargetWorldPosition = null,
            bool reformNativeIssued = false)
        {
            _sourceFormation = sourceFormation;
            TargetFormation = targetFormation;
            CurrentState = currentState;
            TargetPosition = targetPosition;
            ImpactPosition = impactPosition;
            PositionLockReleased = positionLockReleased;
            ReformAllowed = reformAllowed;
            CommanderValid = commanderValid;
            DistanceToTargetFormation = distanceToTargetFormation;
            DistanceFromImpactPosition = distanceFromImpactPosition;
            TimeSinceLockRelease = timeSinceLockRelease;
            Reason = reason ?? string.Empty;
            NativeForwardIssued = nativeForwardIssued;
            NativeChargeIssued = nativeChargeIssued;
            SequenceTimeSeconds = sequenceTimeSeconds;
            Aborted = aborted;
            AbortReason = abortReason ?? string.Empty;
            LastKnownTargetWorldPosition = lastKnownTargetWorldPosition;
            ReformNativeIssued = reformNativeIssued;
        }

        public Formation SourceFormation => _sourceFormation;

        public Formation TargetFormation { get; set; }

        public CavalryChargeState CurrentState { get; set; }

        public Vec3? TargetPosition { get; set; }

        public Vec3? ImpactPosition { get; set; }

        public bool PositionLockReleased { get; set; }

        public bool ReformAllowed { get; set; }

        public bool CommanderValid { get; set; }

        public float DistanceToTargetFormation { get; set; }

        public float DistanceFromImpactPosition { get; set; }

        public float TimeSinceLockRelease { get; set; }

        public string Reason { get; set; }

        public bool NativeForwardIssued { get; set; }

        public bool NativeChargeIssued { get; set; }

        public float SequenceTimeSeconds { get; set; }

        public bool Aborted { get; set; }

        public string AbortReason { get; set; }

        public Vec3? LastKnownTargetWorldPosition { get; set; }

        public bool ReformNativeIssued { get; set; }

        /// <summary>Seconds since sequence start when advance/move was last issued (native sequence).</summary>
        public float LastNativeAdvanceIssueTime { get; set; } = -1f;

        public static CavalryChargeSequenceState NotMounted(Formation formation, string reason)
        {
            return new CavalryChargeSequenceState(
                formation,
                null,
                CavalryChargeState.NotMountedFormation,
                null,
                null,
                false,
                false,
                false,
                float.MaxValue,
                float.MaxValue,
                0f,
                reason ?? string.Empty);
        }
    }
}
