using System.Collections.Generic;
using TaleWorlds.MountAndBlade;

namespace Bannerlord.RTSCameraLite.Selection
{
    /// <summary>
    /// TW-1 friendly formation selection state. It owns selection bookkeeping only; it never issues orders.
    /// </summary>
    public sealed class FormationSelectionState
    {
        private readonly List<Formation> _selected = new List<Formation>();

        public int SelectedCount => _selected.Count;

        public IReadOnlyList<Formation> SelectedFormations => _selected.AsReadOnly();

        public Formation[] SnapshotSelectedFormations()
        {
            return _selected.ToArray();
        }

        public bool TryGetPrimarySelectedFormation(out Formation formation)
        {
            formation = null;
            if (_selected.Count == 0)
            {
                return false;
            }

            Formation candidate = _selected[0];
            if (!IsUsable(candidate))
            {
                _selected.RemoveAt(0);
                return false;
            }

            formation = candidate;
            return true;
        }

        public void Clear()
        {
            _selected.Clear();
        }

        public bool SelectSingle(Formation formation)
        {
            _selected.Clear();
            if (!IsUsable(formation))
            {
                return false;
            }

            _selected.Add(formation);
            return true;
        }

        public bool Add(Formation formation)
        {
            if (!IsUsable(formation) || _selected.Contains(formation))
            {
                return false;
            }

            _selected.Add(formation);
            return true;
        }

        public bool Remove(Formation formation)
        {
            if (formation == null)
            {
                return false;
            }

            return _selected.Remove(formation);
        }

        private static bool IsUsable(Formation formation)
        {
            if (formation == null)
            {
                return false;
            }

            try
            {
                return formation.CountOfUnits > 0;
            }
            catch
            {
                return false;
            }
        }
    }
}
