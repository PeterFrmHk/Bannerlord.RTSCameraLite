using System.Collections.Generic;
using Bannerlord.RTSCameraLite.Commands;
using Bannerlord.RTSCameraLite.Config;

namespace Bannerlord.RTSCameraLite.Doctrine
{
    /// <summary>
    /// Policy layer: commander mode, doctrine context, and formation eligibility vs <see cref="CommandIntent"/> (Slice 15).
    /// </summary>
    public sealed class FormationRestrictionService
    {
        private readonly CommanderConfig _config;

        public FormationRestrictionService(CommanderConfig config)
        {
            _config = config ?? CommanderConfigDefaults.CreateDefault();
        }

        public RestrictionDecision Evaluate(CommandIntent intent, CommandContext context)
        {
            if (context == null)
            {
                return RestrictionDecision.Block("context is null", Singleton(AllowedFormationType.Mob));
            }

            if (!context.CommanderModeEnabled)
            {
                return RestrictionDecision.Block("commander mode disabled", Singleton(AllowedFormationType.Mob));
            }

            if (context.Mission == null)
            {
                return RestrictionDecision.Block("mission is null", Singleton(AllowedFormationType.Mob));
            }

            if (intent == null)
            {
                return RestrictionDecision.Deny("intent is null");
            }

            if (intent.Type == CommandType.None)
            {
                return RestrictionDecision.Allow("none");
            }

            if (intent.SourceFormation == null)
            {
                return RestrictionDecision.Deny("source formation is null");
            }

            if (intent.RequiresPosition && !intent.TargetPosition.HasValue)
            {
                return RestrictionDecision.Deny("command requires a target position");
            }

            if (intent.RequiresTargetFormation && intent.TargetFormation == null)
            {
                return RestrictionDecision.Deny("command requires a target formation");
            }

            bool hasCommander = context.Commander != null && context.Commander.HasCommander;
            FormationEligibilityResult elig = context.Eligibility;

            switch (intent.Type)
            {
                case CommandType.Stop:
                    return RestrictionDecision.Allow("stop");

                case CommandType.BasicHold:
                    if (!hasCommander)
                    {
                        return _config.AllowNoCommanderBasicHold
                            ? RestrictionDecision.Allow("basic hold without commander (config)")
                            : RestrictionDecision.Deny("basic hold denied without commander", Singleton(AllowedFormationType.BasicHold));
                    }

                    return EligibilityAllows(elig, AllowedFormationType.BasicHold)
                        ? RestrictionDecision.Allow("basic hold")
                        : RestrictionDecision.Deny("eligibility does not allow BasicHold", Singleton(AllowedFormationType.BasicHold));

                case CommandType.BasicFollow:
                    if (!hasCommander)
                    {
                        return _config.AllowNoCommanderBasicFollow
                            ? RestrictionDecision.Allow("basic follow without commander (config)")
                            : RestrictionDecision.Deny("basic follow denied without commander", Singleton(AllowedFormationType.BasicFollow));
                    }

                    return EligibilityAllows(elig, AllowedFormationType.BasicFollow)
                        ? RestrictionDecision.Allow("basic follow")
                        : RestrictionDecision.Deny("eligibility does not allow BasicFollow", Singleton(AllowedFormationType.BasicFollow));

                case CommandType.BasicLine:
                case CommandType.Loose:
                case CommandType.ShieldWall:
                case CommandType.Square:
                case CommandType.Circle:
                case CommandType.MountedWide:
                case CommandType.HorseArcherLoose:
                case CommandType.Reform:
                    if (_config.BlockAdvancedCommandsWithoutCommander && !hasCommander)
                    {
                        return RestrictionDecision.Deny("advanced formation command requires commander", RequiredFor(intent.Type));
                    }

                    return EvaluateFormationShape(intent.Type, elig);

                case CommandType.Charge:
                    if (_config.AllowBasicChargeWithoutAdvancedDoctrine)
                    {
                        return RestrictionDecision.Allow("charge allowed by non-doctrine policy");
                    }

                    if (!hasCommander)
                    {
                        return RestrictionDecision.Deny("charge requires commander when doctrine charge is enforced");
                    }

                    return RestrictionDecision.Allow("charge with commander");

                case CommandType.AdvanceOrMove:
                    if (_config.BlockAdvancedCommandsWithoutCommander && !hasCommander)
                    {
                        return RestrictionDecision.Deny("advance/move requires commander", Singleton(AllowedFormationType.Loose));
                    }

                    if (!AllowsMovement(elig))
                    {
                        return RestrictionDecision.Deny("eligibility lacks movement-capable formation orders", Singleton(AllowedFormationType.Loose));
                    }

                    return RestrictionDecision.Allow("advance/move");

                case CommandType.NativeCavalryChargeSequence:
                    if (!hasCommander)
                    {
                        return RestrictionDecision.Deny("cavalry sequence requires commander", Singleton(AllowedFormationType.MountedWide));
                    }

                    if (!IsCavalryHeavy(context))
                    {
                        return RestrictionDecision.Deny("cavalry sequence requires mounted-heavy doctrine/eligibility", Singleton(AllowedFormationType.MountedWide));
                    }

                    if (!EligibilityAllows(elig, AllowedFormationType.MountedWide)
                        && !EligibilityAllows(elig, AllowedFormationType.HorseArcherLoose))
                    {
                        return RestrictionDecision.Deny("cavalry sequence requires mounted or horse-archer eligibility", Singleton(AllowedFormationType.MountedWide));
                    }

                    return RestrictionDecision.Allow("native cavalry sequence policy passed");

                default:
                    return RestrictionDecision.Deny("unsupported command type for restriction layer");
            }
        }

        private static RestrictionDecision EvaluateFormationShape(CommandType type, FormationEligibilityResult elig)
        {
            AllowedFormationType required = MapToAllowed(type);
            if (required == AllowedFormationType.Mob)
            {
                return RestrictionDecision.Allow("no specific formation gate");
            }

            return EligibilityAllows(elig, required)
                ? RestrictionDecision.Allow(type.ToString())
                : RestrictionDecision.Deny($"eligibility does not allow {required}", Singleton(required));
        }

        private static AllowedFormationType MapToAllowed(CommandType type)
        {
            switch (type)
            {
                case CommandType.BasicLine:
                    return AllowedFormationType.BasicLine;
                case CommandType.Loose:
                    return AllowedFormationType.Loose;
                case CommandType.ShieldWall:
                    return AllowedFormationType.ShieldWall;
                case CommandType.Square:
                    return AllowedFormationType.Square;
                case CommandType.Circle:
                    return AllowedFormationType.Circle;
                case CommandType.MountedWide:
                    return AllowedFormationType.MountedWide;
                case CommandType.HorseArcherLoose:
                    return AllowedFormationType.HorseArcherLoose;
                case CommandType.Reform:
                    return AllowedFormationType.BasicHold;
                default:
                    return AllowedFormationType.Mob;
            }
        }

        private static List<AllowedFormationType> RequiredFor(CommandType type)
        {
            AllowedFormationType r = MapToAllowed(type);
            return r == AllowedFormationType.Mob ? new List<AllowedFormationType>() : Singleton(r);
        }

        private static List<AllowedFormationType> Singleton(AllowedFormationType t)
        {
            return new List<AllowedFormationType> { t };
        }

        private static bool EligibilityAllows(FormationEligibilityResult elig, AllowedFormationType t)
        {
            if (elig == null || !elig.Success)
            {
                return false;
            }

            for (int i = 0; i < elig.AllowedFormationTypes.Count; i++)
            {
                if (elig.AllowedFormationTypes[i] == t)
                {
                    return true;
                }
            }

            return false;
        }

        private static bool AllowsMovement(FormationEligibilityResult elig)
        {
            return EligibilityAllows(elig, AllowedFormationType.Loose)
                   || EligibilityAllows(elig, AllowedFormationType.BasicLine)
                   || EligibilityAllows(elig, AllowedFormationType.BasicFollow)
                   || EligibilityAllows(elig, AllowedFormationType.BasicHold);
        }

        private static bool IsCavalryHeavy(CommandContext context)
        {
            if (context?.DoctrineProfile?.Composition == null)
            {
                return false;
            }

            return context.DoctrineProfile.Composition.IsMountedHeavy;
        }
    }
}
