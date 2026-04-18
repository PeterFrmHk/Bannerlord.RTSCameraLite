using System;
using System.Collections.Generic;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;

namespace Bannerlord.RTSCameraLite.Tactical
{
    /// <summary>
    /// Queries friendly formations for the main agent's team. Results are cached for a few ticks.
    /// </summary>
    internal sealed class FormationQueryService
    {
        private const int CacheValidityTicks = 8;

        private readonly List<Formation> _cachedList = new List<Formation>();
        private int _cacheUntilTick = int.MinValue;
        private Team _cacheTeam;

        public IReadOnlyList<Formation> GetFriendlyFormations(TaleWorlds.MountAndBlade.Mission mission, Agent mainAgent, int missionTick)
        {
            if (mission == null || mainAgent == null || mainAgent.Team == null)
            {
                return Array.Empty<Formation>();
            }

            Team playerTeam = mainAgent.Team;
            if (missionTick < _cacheUntilTick && ReferenceEquals(_cacheTeam, playerTeam))
            {
                return _cachedList;
            }

            _cachedList.Clear();
            _cacheTeam = playerTeam;

            try
            {
                foreach (FormationClass formationClass in FormationClassExtensions.OrderedFormationClasses)
                {
                    Formation formation;
                    try
                    {
                        formation = playerTeam.GetFormation(formationClass);
                    }
                    catch
                    {
                        continue;
                    }

                    if (formation == null)
                    {
                        continue;
                    }

                    if (!ReferenceEquals(formation.Team, playerTeam))
                    {
                        continue;
                    }

                    int count;
                    try
                    {
                        count = formation.CountOfUnits;
                    }
                    catch
                    {
                        continue;
                    }

                    if (count <= 0)
                    {
                        continue;
                    }

                    _cachedList.Add(formation);
                }
            }
            catch
            {
                _cachedList.Clear();
            }

            _cacheUntilTick = missionTick + CacheValidityTicks;
            return _cachedList;
        }

        public void InvalidateCache()
        {
            _cacheUntilTick = int.MinValue;
            _cachedList.Clear();
            _cacheTeam = null;
        }
    }

    internal static class FormationClassExtensions
    {
        internal static readonly FormationClass[] OrderedFormationClasses = BuildOrdered();

        private static FormationClass[] BuildOrdered()
        {
            List<FormationClass> list = new List<FormationClass>();
            foreach (FormationClass fc in Enum.GetValues(typeof(FormationClass)))
            {
                if (IsSkippablePlaceholder(fc))
                {
                    continue;
                }

                list.Add(fc);
            }

            list.Sort((a, b) => ((int)a).CompareTo((int)b));
            return list.ToArray();
        }

        private static bool IsSkippablePlaceholder(FormationClass fc)
        {
            string name = fc.ToString();
            if (name.IndexOf("Unset", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return true;
            }

            if (name.IndexOf("Invalid", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return true;
            }

            if (name.IndexOf("None", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return true;
            }

            return false;
        }
    }
}
