using System.Collections.Generic;
using TaleWorlds.MountAndBlade;

namespace Bannerlord.RTSCameraLite.Tactical
{
    /// <summary>
    /// Tracks which friendly formation is selected for camera focus (no orders issued).
    /// </summary>
    internal sealed class FormationSelectionState
    {
        private int _index = -1;

        public int SelectedIndex => _index;

        public Formation SelectedFormation { get; private set; }

        public void ClearIfInvalid(IReadOnlyList<Formation> ordered)
        {
            if (ordered == null || ordered.Count == 0)
            {
                Clear();
                return;
            }

            if (_index < 0 || _index >= ordered.Count)
            {
                Clear();
                return;
            }

            Formation atIndex = ordered[_index];
            if (!IsFormationUsable(atIndex))
            {
                Clear();
                return;
            }

            SelectedFormation = atIndex;
        }

        public void NextFormation(IReadOnlyList<Formation> ordered)
        {
            if (ordered == null || ordered.Count == 0)
            {
                Clear();
                return;
            }

            if (_index < 0)
            {
                _index = 0;
            }
            else
            {
                _index = (_index + 1) % ordered.Count;
            }

            BindSelection(ordered);
        }

        public void PreviousFormation(IReadOnlyList<Formation> ordered)
        {
            if (ordered == null || ordered.Count == 0)
            {
                Clear();
                return;
            }

            if (_index < 0)
            {
                _index = ordered.Count - 1;
            }
            else
            {
                _index = (_index - 1 + ordered.Count) % ordered.Count;
            }

            BindSelection(ordered);
        }

        public void Clear()
        {
            _index = -1;
            SelectedFormation = null;
        }

        /// <summary>
        /// Safe snapshot for UX / feedback (never throws to caller).
        /// </summary>
        /// <summary>
        /// Returns the current usable selected formation for command routing, if any.
        /// </summary>
        public bool TryGetSelectedFormation(out Formation formation)
        {
            formation = SelectedFormation;
            if (formation == null || _index < 0)
            {
                return false;
            }

            return IsFormationUsable(formation);
        }

        public bool TryGetSelectionDetails(out int listIndex, out int unitCount, out string classLabel)
        {
            listIndex = _index;
            unitCount = 0;
            classLabel = string.Empty;

            if (SelectedFormation == null || _index < 0)
            {
                return false;
            }

            try
            {
                unitCount = SelectedFormation.CountOfUnits;
            }
            catch
            {
                unitCount = 0;
            }

            classLabel = $"group-{listIndex + 1}";

            return true;
        }

        private void BindSelection(IReadOnlyList<Formation> ordered)
        {
            SelectedFormation = ordered[_index];
        }

        private static bool IsFormationUsable(Formation formation)
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
