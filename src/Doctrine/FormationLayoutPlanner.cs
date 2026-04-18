using System;
using Bannerlord.RTSCameraLite.Commander;
using Bannerlord.RTSCameraLite.Config;
using Bannerlord.RTSCameraLite.Equipment;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace Bannerlord.RTSCameraLite.Doctrine
{
    /// <summary>
    /// Derives a coarse row/rank spacing plan from doctrine, eligibility, and composition (Slice 12+13 — planning only).
    /// </summary>
    public static class FormationLayoutPlanner
    {
        public static RowRankSpacingPlan Build(
            Formation formation,
            FormationDoctrineProfile doctrine,
            FormationEligibilityResult eligibility,
            FormationCompositionProfile composition,
            CommanderPresenceResult presence,
            CommanderConfig commanderConfig = null)
        {
            CommanderConfig cfg = commanderConfig ?? CommanderConfigDefaults.CreateDefault();

            if (formation == null || doctrine == null || eligibility == null || composition == null)
            {
                return new RowRankSpacingPlan(
                    1,
                    1,
                    1.2f,
                    1.5f,
                    useLooseSpacing: true,
                    useTightSpacing: false,
                    "default",
                    "default",
                    "default",
                    0f,
                    "invalid inputs",
                    isMountedLayout: false,
                    isHorseArcherLayout: false,
                    positionLockAllowed: false,
                    releaseLockAfterCloseContact: false,
                    reformDistance: cfg.CavalryReformDistanceFromAttackedFormation,
                    mountedDoctrineReason: string.Empty);
            }

            try
            {
                int count = Math.Max(1, formation.CountOfUnits);
                int rows = MBMath.ClampInt((int)Math.Ceiling(Math.Sqrt(count)), 2, 8);
                int ranks = MBMath.ClampInt(count / Math.Max(1, rows), 1, 6);

                float mountedShare = composition.CavalryRatio + composition.HorseArcherRatio;
                bool mountedHeavy = composition.IsMountedHeavy || mountedShare >= 0.45f;
                bool horseArcherHeavy = CavalrySpacingRules.IsHorseArcherHeavyFormation(composition);
                bool shockCavalryHeavy = CavalrySpacingRules.IsShockCavalryHeavyFormation(composition, formation);
                bool rangedHeavy = composition.HorseArcherRatio >= 0.35f || formation.RepresentativeClass == FormationClass.Ranged;
                float discipline = doctrine.FormationDisciplineScore;
                bool loose = discipline < 0.35f
                             || doctrine.MoraleScore < 0.28f
                             || doctrine.CasualtyShock > 0.55f
                             || !eligibility.Success;

                float lateral = mountedHeavy ? 2.4f : rangedHeavy ? 2.0f : 1.2f;
                float depth = mountedHeavy ? 3.0f : 1.6f;
                float trainingTighten = MBMath.ClampFloat(0.9f + doctrine.TrainingScore * 0.25f, 0.85f, 1.2f);
                lateral *= trainingTighten;
                depth *= MBMath.ClampFloat(0.95f + doctrine.RankQualityScore * 0.15f, 0.9f, 1.15f);

                string front = "shields-high-morale";
                string second = "polearms-shock";
                string rear = "low-morale-wounded";

                if (rangedHeavy && !horseArcherHeavy)
                {
                    front = "skirmisher-edge";
                    second = "ranged-support";
                    rear = "reserve";
                }

                bool isMountedLayout = false;
                bool isHorseArcherLayout = false;
                bool positionLockAllowed = false;
                bool releaseLockAfterCloseContact = false;
                string mountedDoctrineReason = string.Empty;

                if (CavalrySpacingRules.IsCavalryHeavyFormation(composition))
                {
                    isMountedLayout = true;
                    isHorseArcherLayout = horseArcherHeavy;
                    CavalrySpacingRules.ApplyMountedSpacing(
                        formation,
                        composition,
                        doctrine,
                        cfg,
                        horseArcherHeavy,
                        shockCavalryHeavy,
                        ref lateral,
                        ref depth,
                        out mountedDoctrineReason);

                    bool commanderOk = presence != null && presence.HasCommander;
                    positionLockAllowed = CavalryPositionLockPolicy.ShouldAllowPositionLock(
                                              CavalryChargeState.RallyingToCommander,
                                              horseArcherHeavy,
                                              commanderOk)
                                          || CavalryPositionLockPolicy.ShouldAllowPositionLock(
                                              CavalryChargeState.MountedFormationAssembling,
                                              horseArcherHeavy,
                                              commanderOk)
                                          || CavalryPositionLockPolicy.ShouldAllowPositionLock(
                                              CavalryChargeState.ChargeReady,
                                              horseArcherHeavy,
                                              commanderOk);

                    releaseLockAfterCloseContact = shockCavalryHeavy && !horseArcherHeavy;
                }

                float confidence = discipline * 0.28f
                                   + doctrine.MoraleScore * 0.12f
                                   + doctrine.TrainingScore * 0.1f
                                   + doctrine.EquipmentScore * 0.08f
                                   + doctrine.RankQualityScore * 0.1f
                                   + (composition.IsCertain ? 0.18f : 0.06f);
                if (presence != null && presence.HasCommander && presence.Commander != null)
                {
                    confidence += presence.Commander.CommandAuthorityScore * 0.22f;
                }

                if (eligibility.Success && eligibility.IsCertain)
                {
                    confidence += 0.12f;
                }

                if (isHorseArcherLayout)
                {
                    confidence *= MBMath.ClampFloat(0.85f + doctrine.MoraleScore * 0.2f, 0.7f, 1f);
                }

                confidence = MBMath.ClampFloat(confidence, 0f, 1f);

                return new RowRankSpacingPlan(
                    rows,
                    ranks,
                    lateral,
                    depth,
                    useLooseSpacing: loose || mountedHeavy,
                    useTightSpacing: !loose && discipline >= 0.55f,
                    front,
                    second,
                    rear,
                    confidence,
                    isMountedLayout ? "mounted " + mountedDoctrineReason : "heuristic layout",
                    isMountedLayout,
                    isHorseArcherLayout,
                    positionLockAllowed,
                    releaseLockAfterCloseContact,
                    cfg.CavalryReformDistanceFromAttackedFormation,
                    mountedDoctrineReason);
            }
            catch (Exception ex)
            {
                return new RowRankSpacingPlan(
                    2,
                    2,
                    1.2f,
                    1.5f,
                    true,
                    false,
                    "fallback",
                    "fallback",
                    "fallback",
                    0.2f,
                    ex.Message,
                    isMountedLayout: false,
                    isHorseArcherLayout: false,
                    positionLockAllowed: false,
                    releaseLockAfterCloseContact: false,
                    reformDistance: cfg.CavalryReformDistanceFromAttackedFormation,
                    mountedDoctrineReason: string.Empty);
            }
        }
    }
}
