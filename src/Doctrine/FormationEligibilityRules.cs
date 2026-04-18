using System;
using System.Collections.Generic;
using Bannerlord.RTSCameraLite.Commander;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace Bannerlord.RTSCameraLite.Doctrine
{
    /// <summary>
    /// Applies formation eligibility from commander presence and doctrine (Slice 11 — advisory only).
    /// </summary>
    public sealed class FormationEligibilityRules
    {
        private static readonly AllowedFormationType[] AllTypes = (AllowedFormationType[])Enum.GetValues(typeof(AllowedFormationType));

        private readonly FormationEligibilitySettings _settings;

        public FormationEligibilityRules(FormationEligibilitySettings settings)
        {
            _settings = settings ?? FormationEligibilitySettings.FromConfig(null);
        }

        public FormationEligibilityResult Evaluate(
            Formation formation,
            CommanderPresenceResult commander,
            FormationDoctrineProfile doctrine)
        {
            if (formation == null)
            {
                return FormationEligibilityResult.Failure("formation is null");
            }

            if (commander == null)
            {
                return FormationEligibilityResult.Uncertain(
                    "commander presence null",
                    new List<AllowedFormationType> { AllowedFormationType.Mob },
                    BuildDenied(new List<AllowedFormationType> { AllowedFormationType.Mob }),
                    hasAdvancedFormationAccess: false);
            }

            if (doctrine == null)
            {
                return FormationEligibilityResult.Uncertain(
                    "doctrine profile null",
                    new List<AllowedFormationType> { AllowedFormationType.Mob },
                    BuildDenied(new List<AllowedFormationType> { AllowedFormationType.Mob }),
                    hasAdvancedFormationAccess: false);
            }

            FormationCompositionProfile comp = doctrine.Composition ?? new FormationCompositionProfile(0f, 0f, 0f, 0f, 0f, false, "missing composition");

            var allowed = new List<AllowedFormationType>();
            float d = doctrine.DisciplineScore;

            allowed.Add(AllowedFormationType.Mob);

            bool hasCommander = commander.HasCommander && commander.Commander != null;

            if (!hasCommander)
            {
                if (_settings.NoCommanderAllowsBasicMobOrders)
                {
                    allowed.Add(AllowedFormationType.BasicHold);
                }

                bool certain = commander.IsCertain && doctrine.IsCertain && comp.IsCertain;
                return Finalize(allowed, certain, "no commander — disciplined shapes denied");
            }

            allowed.Add(AllowedFormationType.BasicHold);
            allowed.Add(AllowedFormationType.BasicFollow);

            if (d >= _settings.BasicLineMinimumDiscipline)
            {
                allowed.Add(AllowedFormationType.BasicLine);
            }

            if (d >= _settings.LooseMinimumDiscipline)
            {
                allowed.Add(AllowedFormationType.Loose);
            }

            if (d >= _settings.ShieldWallMinimumDiscipline
                && comp.ShieldRatio >= _settings.MinimumShieldRatioForShieldWall)
            {
                allowed.Add(AllowedFormationType.ShieldWall);
            }

            float poleOrShield = MBMath.ClampFloat(comp.PolearmRatio + comp.ShieldRatio, 0f, 2f);
            if (d >= _settings.SquareMinimumDiscipline
                && poleOrShield >= _settings.MinimumPolearmOrShieldRatioForSquare)
            {
                allowed.Add(AllowedFormationType.Square);
            }

            if (d >= _settings.CircleMinimumDiscipline)
            {
                allowed.Add(AllowedFormationType.Circle);
                allowed.Add(AllowedFormationType.Skein);
            }

            if (d >= _settings.LooseMinimumDiscipline
                && comp.CavalryRatio >= _settings.MinimumMountedRatioForMountedWide)
            {
                allowed.Add(AllowedFormationType.MountedWide);
            }

            if (d >= _settings.LooseMinimumDiscipline
                && comp.HorseArcherRatio >= _settings.MinimumHorseArcherRatioForHorseArcherLoose)
            {
                allowed.Add(AllowedFormationType.HorseArcherLoose);
            }

            bool heroOrCaptain = commander.Commander.IsHero || commander.Commander.IsCaptain;
            if (heroOrCaptain
                && d >= _settings.AdvancedAdaptiveMinimumDiscipline
                && commander.IsCertain
                && doctrine.IsCertain
                && comp.IsCertain)
            {
                allowed.Add(AllowedFormationType.AdvancedAdaptive);
            }

            bool isCertain = commander.IsCertain && doctrine.IsCertain && comp.IsCertain;
            return Finalize(allowed, isCertain, "evaluated");
        }

        private FormationEligibilityResult Finalize(List<AllowedFormationType> allowed, bool isCertain, string reason)
        {
            List<AllowedFormationType> denied = BuildDenied(allowed);
            bool hasAdvanced = allowed.Contains(AllowedFormationType.AdvancedAdaptive);
            return FormationEligibilityResult.CreateSuccess(hasAdvanced, allowed, denied, isCertain, reason);
        }

        private static List<AllowedFormationType> BuildDenied(List<AllowedFormationType> allowed)
        {
            var set = new HashSet<AllowedFormationType>(allowed);
            var denied = new List<AllowedFormationType>();
            for (int i = 0; i < AllTypes.Length; i++)
            {
                if (!set.Contains(AllTypes[i]))
                {
                    denied.Add(AllTypes[i]);
                }
            }

            denied.Sort((a, b) => a.CompareTo(b));
            allowed.Sort((a, b) => a.CompareTo(b));
            return denied;
        }
    }
}
