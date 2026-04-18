using System;
using Bannerlord.RTSCameraLite.Adapters;
using TaleWorlds.MountAndBlade;

namespace Bannerlord.RTSCameraLite.Equipment
{
    /// <summary>
    /// Maps agents to <see cref="EquipmentRole"/> using <see cref="FormationDataAdapter"/> only for engine reads.
    /// </summary>
    public static class EquipmentRoleClassifier
    {
        public static EquipmentRole Classify(FormationDataAdapter adapter, Agent agent)
        {
            if (adapter == null || agent == null)
            {
                return EquipmentRole.Unknown;
            }

            try
            {
                FormationDataResult hints = adapter.TryGetAgentEquipmentHints(agent);
                if (!hints.Success)
                {
                    return EquipmentRole.Unknown;
                }

                bool mounted = hints.CommanderLikely;
                bool shield = hints.FloatValueB > 0.5f;
                string wc = hints.Message ?? string.Empty;

                if (mounted)
                {
                    if (wc.IndexOf("bow", StringComparison.OrdinalIgnoreCase) >= 0
                        || wc.IndexOf("javelin", StringComparison.OrdinalIgnoreCase) >= 0
                        || wc.IndexOf("throw", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        return EquipmentRole.HorseArcher;
                    }

                    return EquipmentRole.Cavalry;
                }

                if (wc.IndexOf("bow", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    return EquipmentRole.Archer;
                }

                if (wc.IndexOf("crossbow", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    return EquipmentRole.Crossbow;
                }

                if (wc.IndexOf("thrown", StringComparison.OrdinalIgnoreCase) >= 0
                    || wc.IndexOf("javelin", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    return EquipmentRole.Skirmisher;
                }

                if (shield)
                {
                    return EquipmentRole.ShieldInfantry;
                }

                if (wc.IndexOf("polearm", StringComparison.OrdinalIgnoreCase) >= 0
                    || wc.IndexOf("twohanded", StringComparison.OrdinalIgnoreCase) >= 0 && wc.IndexOf("axe", StringComparison.OrdinalIgnoreCase) < 0)
                {
                    return EquipmentRole.Polearm;
                }

                if (wc.IndexOf("twohanded", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    return EquipmentRole.ShockInfantry;
                }

                return EquipmentRole.Unknown;
            }
            catch
            {
                return EquipmentRole.Unknown;
            }
        }
    }
}
