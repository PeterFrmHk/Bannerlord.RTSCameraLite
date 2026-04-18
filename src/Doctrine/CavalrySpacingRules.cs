using System;
using Bannerlord.RTSCameraLite.Config;
using Bannerlord.RTSCameraLite.Equipment;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace Bannerlord.RTSCameraLite.Doctrine
{
    /// <summary>
    /// Mounted / horse-archer spacing heuristics (Slice 13 — data only).
    /// </summary>
    public static class CavalrySpacingRules
    {
        public static bool IsHorseArcherHeavyFormation(FormationCompositionProfile composition)
        {
            if (composition == null)
            {
                return false;
            }

            try
            {
                return composition.HorseArcherRatio >= 0.35f
                       || (composition.HorseArcherRatio >= 0.28f && composition.HorseArcherRatio > composition.CavalryRatio);
            }
            catch
            {
                return false;
            }
        }

        public static bool IsCavalryHeavyFormation(FormationCompositionProfile composition)
        {
            if (composition == null)
            {
                return false;
            }

            try
            {
                float mounted = composition.CavalryRatio + composition.HorseArcherRatio;
                return composition.IsMountedHeavy || mounted >= 0.45f;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// True when formation is mostly melee cavalry (shock path), not horse-archer skirmish line.
        /// </summary>
        public static bool IsShockCavalryHeavyFormation(FormationCompositionProfile composition, Formation formation)
        {
            if (composition == null)
            {
                return false;
            }

            try
            {
                if (IsHorseArcherHeavyFormation(composition))
                {
                    return false;
                }

                if (formation != null && formation.RepresentativeClass == FormationClass.Cavalry)
                {
                    return composition.CavalryRatio >= 0.3f;
                }

                return composition.CavalryRatio >= 0.35f && composition.HorseArcherRatio < 0.22f;
            }
            catch
            {
                return false;
            }
        }

        public static bool IsHeavyCavalryDominant(FormationCompositionProfile composition)
        {
            if (composition == null)
            {
                return false;
            }

            try
            {
                return composition.CavalryRatio >= 0.35f
                       && composition.HeavyArmorEstimate >= 0.45f
                       && composition.HorseArcherRatio < 0.2f;
            }
            catch
            {
                return false;
            }
        }

        public static bool IsLightCavalryDominant(FormationCompositionProfile composition)
        {
            if (composition == null)
            {
                return false;
            }

            try
            {
                if (IsHorseArcherHeavyFormation(composition) || IsHeavyCavalryDominant(composition))
                {
                    return false;
                }

                float mounted = composition.CavalryRatio + composition.HorseArcherRatio;
                return mounted >= 0.45f && composition.CavalryRatio >= 0.2f;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Computes lateral/depth spacing for mounted layouts; callers merge rows/ranks from infantry heuristic if needed.
        /// </summary>
        public static void ApplyMountedSpacing(
            Formation formation,
            FormationCompositionProfile composition,
            FormationDoctrineProfile doctrine,
            CommanderConfig config,
            bool isHorseArcherHeavy,
            bool isShockCavalryHeavy,
            ref float lateralSpacing,
            ref float depthSpacing,
            out string reason)
        {
            CommanderConfig c = config ?? CommanderConfigDefaults.CreateDefault();
            reason = string.Empty;
            try
            {
                if (isHorseArcherHeavy)
                {
                    lateralSpacing = Math.Max(lateralSpacing, c.HorseArcherLateralSpacing);
                    depthSpacing = Math.Max(depthSpacing, c.HorseArcherDepthSpacing);
                    reason = "horse-archer-wide";
                    return;
                }

                float lat = c.CavalryLateralSpacing;
                float dep = c.CavalryDepthSpacing;
                if (IsHeavyCavalryDominant(composition))
                {
                    lat *= 1.08f;
                    dep *= 1.12f;
                    reason = "heavy-cavalry-wide";
                }
                else if (IsLightCavalryDominant(composition))
                {
                    lat *= 1.15f;
                    dep *= 1.05f;
                    reason = "light-cavalry-wide-lateral";
                }
                else
                {
                    reason = isShockCavalryHeavy ? "shock-cavalry-wide" : "mixed-mounted-wide";
                }

                float trainingPenalty = MBMath.ClampFloat(1.15f - doctrine.TrainingScore * 0.35f, 1f, 1.25f);
                float moralePenalty = MBMath.ClampFloat(1.1f - doctrine.MoraleScore * 0.25f, 1f, 1.2f);
                lat *= trainingPenalty * moralePenalty;
                dep *= trainingPenalty * moralePenalty;

                lateralSpacing = Math.Max(lateralSpacing, lat);
                depthSpacing = Math.Max(depthSpacing, dep);
            }
            catch (Exception ex)
            {
                lateralSpacing = Math.Max(lateralSpacing, c.CavalryLateralSpacing);
                depthSpacing = Math.Max(depthSpacing, c.CavalryDepthSpacing);
                reason = "fallback: " + ex.Message;
            }
        }
    }
}
