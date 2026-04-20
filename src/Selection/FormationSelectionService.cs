using System;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;

namespace Bannerlord.RTSCameraLite.Selection
{
    /// <summary>
    /// TW-1 number-key formation lookup. This class selects state only; it never mutates formations or issues orders.
    /// </summary>
    public sealed class FormationSelectionService
    {
        public FormationSelectionResult TrySelectNumberKeySlot(
            TaleWorlds.MountAndBlade.Mission mission,
            bool runtimeHooksEnabled,
            bool formationSelectionEnabled,
            int slot,
            FormationSelectionState state)
        {
            try
            {
                if (!runtimeHooksEnabled || !formationSelectionEnabled)
                {
                    return FormationSelectionResult.NotHandled("Formation selection disabled.");
                }

                if (slot < 1 || slot > 8)
                {
                    return FormationSelectionResult.NotHandled("Formation selection slot out of range.");
                }

                if (state == null)
                {
                    return FormationSelectionResult.Failed(slot, GetSlotLabel(slot), "Formation selection state unavailable.");
                }

                string label = GetSlotLabel(slot);
                if (slot > 4)
                {
                    return FormationSelectionResult.Failed(slot, label, $"{label} selection is reserved for a future slice.");
                }

                if (mission == null || mission.MissionEnded)
                {
                    return FormationSelectionResult.Failed(slot, label, $"No {label} formation available.");
                }

                Team playerTeam;
                try
                {
                    playerTeam = mission.PlayerTeam;
                }
                catch
                {
                    return FormationSelectionResult.Failed(slot, label, $"No {label} formation available.");
                }

                if (playerTeam == null)
                {
                    return FormationSelectionResult.Failed(slot, label, $"No {label} formation available.");
                }

                Formation selected = TryFindMatchingFormation(playerTeam, slot);
                if (selected == null)
                {
                    state.Clear();
                    return FormationSelectionResult.Failed(slot, label, $"No {label} formation available.");
                }

                int units = SafeUnitCount(selected);
                bool stored = state.SelectSingle(selected);
                if (!stored)
                {
                    return FormationSelectionResult.Failed(slot, label, $"No {label} formation available.");
                }

                return FormationSelectionResult.Selected(
                    slot,
                    label,
                    selected,
                    units,
                    $"Selected {label} formation: {units} troops");
            }
            catch (Exception ex)
            {
                try
                {
                    state?.Clear();
                }
                catch
                {
                    // Ignore cleanup failure; selection must fail closed.
                }

                return FormationSelectionResult.Failed(slot, GetSlotLabel(slot), $"Formation selection failed safely: {ex.Message}");
            }
        }

        private static Formation TryFindMatchingFormation(Team playerTeam, int slot)
        {
            try
            {
                foreach (Formation formation in playerTeam.FormationsIncludingEmpty)
                {
                    if (formation == null)
                    {
                        continue;
                    }

                    if (!ReferenceEquals(formation.Team, playerTeam))
                    {
                        continue;
                    }

                    if (SafeUnitCount(formation) <= 0)
                    {
                        continue;
                    }

                    if (MatchesSlot(formation, slot))
                    {
                        return formation;
                    }
                }
            }
            catch
            {
                return null;
            }

            return null;
        }

        private static bool MatchesSlot(Formation formation, int slot)
        {
            FormationClass formationClass;
            try
            {
                formationClass = formation.RepresentativeClass;
            }
            catch
            {
                return false;
            }

            switch (slot)
            {
                case 1:
                    return formationClass == FormationClass.Infantry
                        || formationClass == FormationClass.HeavyInfantry;
                case 2:
                    return formationClass == FormationClass.Ranged
                        || formationClass == FormationClass.Skirmisher;
                case 3:
                    return formationClass == FormationClass.Cavalry
                        || formationClass == FormationClass.LightCavalry
                        || formationClass == FormationClass.HeavyCavalry;
                case 4:
                    return formationClass == FormationClass.HorseArcher;
                default:
                    return false;
            }
        }

        private static int SafeUnitCount(Formation formation)
        {
            if (formation == null)
            {
                return 0;
            }

            try
            {
                return formation.CountOfUnits;
            }
            catch
            {
                return 0;
            }
        }

        private static string GetSlotLabel(int slot)
        {
            switch (slot)
            {
                case 1:
                    return "Infantry";
                case 2:
                    return "Ranged";
                case 3:
                    return "Cavalry";
                case 4:
                    return "Horse Archer";
                case 5:
                case 6:
                case 7:
                case 8:
                    return "Reserved";
                default:
                    return "Unknown";
            }
        }
    }

    public sealed class FormationSelectionResult
    {
        private FormationSelectionResult(
            bool handled,
            bool success,
            int slot,
            string label,
            Formation formation,
            int unitCount,
            string message)
        {
            Handled = handled;
            Success = success;
            Slot = slot;
            Label = label ?? string.Empty;
            Formation = formation;
            UnitCount = unitCount;
            Message = message ?? string.Empty;
        }

        public bool Handled { get; }

        public bool Success { get; }

        public int Slot { get; }

        public string Label { get; }

        public Formation Formation { get; }

        public int UnitCount { get; }

        public string Message { get; }

        public static FormationSelectionResult NotHandled(string message)
        {
            return new FormationSelectionResult(false, false, 0, string.Empty, null, 0, message);
        }

        public static FormationSelectionResult Failed(int slot, string label, string message)
        {
            return new FormationSelectionResult(true, false, slot, label, null, 0, message);
        }

        public static FormationSelectionResult Selected(
            int slot,
            string label,
            Formation formation,
            int unitCount,
            string message)
        {
            return new FormationSelectionResult(true, true, slot, label, formation, unitCount, message);
        }
    }
}
