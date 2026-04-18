using System;
using System.Collections.Generic;
using Bannerlord.RTSCameraLite.Adapters;
using Bannerlord.RTSCameraLite.Commander;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace Bannerlord.RTSCameraLite.Doctrine
{
    /// <summary>
    /// Builds composition + doctrine scores from public agent/formation surfaces (Slice 11).
    /// </summary>
    public static class DoctrineScoreCalculator
    {
        /// <summary>
        /// Computes doctrine profile (includes composition). Never throws to callers.
        /// </summary>
        public static FormationDoctrineProfile Compute(
            Formation formation,
            CommanderPresenceResult presence,
            FormationDataAdapter adapter)
        {
            if (formation == null || adapter == null)
            {
                return new FormationDoctrineProfile(
                    0f,
                    0f,
                    0f,
                    0f,
                    0f,
                    new FormationCompositionProfile(0f, 0f, 0f, 0f, 0f, false, "null formation or adapter"),
                    isCertain: false,
                    "invalid inputs");
            }

            FormationDataResult agentsResult = adapter.TryGetFormationAgents(formation);
            if (!agentsResult.Success || agentsResult.Agents.Count == 0)
            {
                return new FormationDoctrineProfile(
                    0f,
                    0f,
                    0f,
                    0f,
                    0f,
                    new FormationCompositionProfile(0f, 0f, 0f, 0f, 0f, false, agentsResult.Message),
                    isCertain: false,
                    "no agents for doctrine scan");
            }

            bool compositionCertain = true;
            int n = 0;
            int shield = 0;
            int polearm = 0;
            int mounted = 0;
            int horseArcher = 0;
            int meleeMounted = 0;
            float moraleAcc = 0f;
            float trainingAcc = 0f;
            float equipAcc = 0f;
            float rankAcc = 0f;

            for (int i = 0; i < agentsResult.Agents.Count; i++)
            {
                Agent agent = agentsResult.Agents[i];
                if (agent == null || !agent.IsActive())
                {
                    continue;
                }

                n++;
                try
                {
                    moraleAcc += MBMath.ClampFloat(agent.Health / Math.Max(1f, agent.HealthLimit), 0f, 1f);
                }
                catch
                {
                    compositionCertain = false;
                    moraleAcc += 0.5f;
                }

                try
                {
                    if (agent.Character is BasicCharacterObject basic)
                    {
                        trainingAcc += MBMath.ClampFloat(basic.Level / 40f, 0f, 1f);
                        rankAcc += MBMath.ClampFloat(basic.Level / 40f, 0f, 1f);
                    }
                    else
                    {
                        trainingAcc += 0.35f;
                        rankAcc += 0.35f;
                        compositionCertain = false;
                    }
                }
                catch
                {
                    trainingAcc += 0.35f;
                    rankAcc += 0.35f;
                    compositionCertain = false;
                }

                float equipGuess = 0.5f;
                try
                {
                    equipGuess = EstimateEquipmentQuality(agent, ref compositionCertain);
                }
                catch
                {
                    compositionCertain = false;
                }

                equipAcc += equipGuess;

                ClassifyWeapons(agent, ref shield, ref polearm, ref mounted, ref horseArcher, ref meleeMounted, ref compositionCertain);
            }

            if (n <= 0)
            {
                return new FormationDoctrineProfile(
                    0f,
                    0f,
                    0f,
                    0f,
                    0f,
                    new FormationCompositionProfile(0f, 0f, 0f, 0f, 0f, false, "no active agents"),
                    false,
                    "no active agents");
            }

            float inv = 1f / n;
            float shieldRatio = shield * inv;
            float polearmRatio = polearm * inv;
            float mountedRatio = mounted * inv;
            float horseArcherRatio = horseArcher * inv;
            float cavalryRatio = ComputeCavalryRatio(formation, mountedRatio, horseArcherRatio, meleeMounted, n);

            var composition = new FormationCompositionProfile(
                shieldRatio,
                polearmRatio,
                mountedRatio,
                cavalryRatio,
                horseArcherRatio,
                compositionCertain,
                compositionCertain ? "composition ok" : "partial equipment read failures");

            float morale01 = moraleAcc * inv;
            float training01 = trainingAcc * inv;
            float equipment01 = equipAcc * inv;
            float rank01 = rankAcc * inv;

            float commandBoost = 0f;
            if (presence != null && presence.HasCommander && presence.Commander != null)
            {
                commandBoost = MBMath.ClampFloat(
                    presence.Commander.LeadershipScore * 0.12f + presence.Commander.TacticsScore * 0.12f + presence.Commander.CommandAuthorityScore * 0.08f,
                    0f,
                    0.35f);
            }

            float discipline = MBMath.ClampFloat(
                morale01 * 0.28f + training01 * 0.24f + equipment01 * 0.2f + rank01 * 0.18f + commandBoost,
                0f,
                1f);

            bool doctrineCertain = compositionCertain && presence != null && presence.IsCertain;
            string reason = doctrineCertain ? "doctrine computed" : "doctrine partially estimated";

            return new FormationDoctrineProfile(
                discipline,
                morale01,
                training01,
                equipment01,
                rank01,
                composition,
                doctrineCertain,
                reason);
        }

        private static float ComputeCavalryRatio(
            Formation formation,
            float mountedRatio,
            float horseArcherRatio,
            int meleeMounted,
            int total)
        {
            try
            {
                FormationClass cls = formation.RepresentativeClass;
                if (cls == FormationClass.Cavalry)
                {
                    return mountedRatio;
                }

                if (cls == FormationClass.HorseArcher)
                {
                    return mountedRatio;
                }

                float meleeMountedRatio = total > 0 ? meleeMounted / (float)total : 0f;
                return MBMath.ClampFloat(Math.Max(meleeMountedRatio, mountedRatio - horseArcherRatio * 0.85f), 0f, 1f);
            }
            catch
            {
                return MBMath.ClampFloat(mountedRatio - horseArcherRatio * 0.85f, 0f, 1f);
            }
        }

        private static float EstimateEquipmentQuality(Agent agent, ref bool certain)
        {
            float sum = 0f;
            int count = 0;
            try
            {
                for (int si = (int)EquipmentIndex.Weapon0; si < (int)EquipmentIndex.NumPrimaryWeaponSlots; si++)
                {
                    MissionWeapon mw = agent.Equipment[(EquipmentIndex)si];
                    if (mw.IsEmpty)
                    {
                        continue;
                    }

                    ItemObject item = mw.Item;
                    if (item == null)
                    {
                        continue;
                    }

                    count++;
                    sum += MBMath.ClampFloat(item.Value / 15000f, 0f, 1f);
                }
            }
            catch
            {
                certain = false;
                return 0.45f;
            }

            if (count == 0)
            {
                certain = false;
                return 0.45f;
            }

            return MBMath.ClampFloat(sum / count, 0f, 1f);
        }

        private static void ClassifyWeapons(
            Agent agent,
            ref int shield,
            ref int polearm,
            ref int mounted,
            ref int horseArcher,
            ref int meleeMounted,
            ref bool certain)
        {
            try
            {
                bool isMounted = agent.HasMount;
                if (isMounted)
                {
                    mounted++;
                }

                bool hasRangedMount = false;
                bool hasMeleeMount = false;
                bool countedShield = false;
                bool countedPolearm = false;

                for (int si = (int)EquipmentIndex.Weapon0; si < (int)EquipmentIndex.NumPrimaryWeaponSlots; si++)
                {
                    MissionWeapon mw = agent.Equipment[(EquipmentIndex)si];
                    if (mw.IsEmpty)
                    {
                        continue;
                    }

                    WeaponClass wc = mw.CurrentUsageItem.WeaponClass;
                    if (!countedShield && (wc == WeaponClass.SmallShield || wc == WeaponClass.LargeShield))
                    {
                        shield++;
                        countedShield = true;
                    }

                    if (!countedPolearm && (wc == WeaponClass.OneHandedPolearm || wc == WeaponClass.TwoHandedPolearm || wc == WeaponClass.LowGripPolearm))
                    {
                        polearm++;
                        countedPolearm = true;
                    }

                    if (isMounted && (wc == WeaponClass.Bow || wc == WeaponClass.Crossbow))
                    {
                        hasRangedMount = true;
                    }

                    if (isMounted && IsMeleeClass(wc))
                    {
                        hasMeleeMount = true;
                    }
                }

                if (isMounted && hasRangedMount)
                {
                    horseArcher++;
                }

                if (isMounted && hasMeleeMount && !hasRangedMount)
                {
                    meleeMounted++;
                }
            }
            catch
            {
                certain = false;
            }
        }

        private static bool IsMeleeClass(WeaponClass wc)
        {
            switch (wc)
            {
                case WeaponClass.OneHandedSword:
                case WeaponClass.TwoHandedSword:
                case WeaponClass.OneHandedAxe:
                case WeaponClass.TwoHandedAxe:
                case WeaponClass.Mace:
                case WeaponClass.TwoHandedMace:
                case WeaponClass.OneHandedPolearm:
                case WeaponClass.TwoHandedPolearm:
                case WeaponClass.LowGripPolearm:
                case WeaponClass.Pick:
                    return true;
                default:
                    return false;
            }
        }
    }
}
