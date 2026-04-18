namespace Bannerlord.RTSCameraLite.Doctrine
{
    /// <summary>
    /// When cavalry position lock is advisory-allowed vs should release vs may re-lock (Slice 13 — no engine calls).
    /// </summary>
    public static class CavalryPositionLockPolicy
    {
        public static bool ShouldAllowPositionLock(
            CavalryChargeState currentState,
            bool isHorseArcherLayout,
            bool commanderValidOrFallback)
        {
            if (isHorseArcherLayout)
            {
                return currentState == CavalryChargeState.RallyingToCommander
                       || currentState == CavalryChargeState.MountedFormationAssembling
                       || currentState == CavalryChargeState.ChargeReady;
            }

            switch (currentState)
            {
                case CavalryChargeState.RallyingToCommander:
                case CavalryChargeState.MountedFormationAssembling:
                case CavalryChargeState.ChargeReady:
                    return true;
                case CavalryChargeState.Charging:
                    return commanderValidOrFallback;
                default:
                    return false;
            }
        }

        public static bool ShouldReleasePositionLock(
            float distanceToTargetFormation,
            bool impactOrCloseContact,
            bool isShockCavalryLayout,
            float releaseLockDistance)
        {
            if (distanceToTargetFormation <= releaseLockDistance)
            {
                return true;
            }

            if (isShockCavalryLayout && impactOrCloseContact)
            {
                return true;
            }

            return false;
        }

        public static bool ShouldReactivatePositionLock(
            bool positionLockCurrentlyReleased,
            float distanceToPrimaryTarget,
            float distanceFromImpactPosition,
            float timeSinceLockReleaseSeconds,
            float reformDistance,
            float reformCooldownSeconds,
            bool reformDisciplineAllowed,
            float releaseLockDistance)
        {
            if (!positionLockCurrentlyReleased)
            {
                return false;
            }

            if (!reformDisciplineAllowed)
            {
                return false;
            }

            if (timeSinceLockReleaseSeconds < reformCooldownSeconds)
            {
                return false;
            }

            float disengage = distanceFromImpactPosition;
            if (float.IsNaN(disengage) || float.IsInfinity(disengage))
            {
                disengage = distanceToPrimaryTarget;
            }

            if (disengage < reformDistance)
            {
                return false;
            }

            if (distanceToPrimaryTarget <= releaseLockDistance)
            {
                return false;
            }

            return true;
        }
    }
}
