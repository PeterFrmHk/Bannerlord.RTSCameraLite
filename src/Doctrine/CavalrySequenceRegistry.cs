using System;
using System.Collections.Generic;
using TaleWorlds.MountAndBlade;

namespace Bannerlord.RTSCameraLite.Doctrine
{
    /// <summary>Tracks one active native cavalry sequence per player formation (Slice 16).</summary>
    public sealed class CavalrySequenceRegistry
    {
        private readonly Dictionary<Formation, CavalryChargeSequenceState> _active =
            new Dictionary<Formation, CavalryChargeSequenceState>();

        public bool StartSequence(Formation sourceFormation, CavalryChargeSequenceState state)
        {
            try
            {
                if (sourceFormation == null || state == null)
                {
                    return false;
                }

                _active[sourceFormation] = state;
                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool StopSequence(Formation sourceFormation)
        {
            try
            {
                if (sourceFormation == null)
                {
                    return false;
                }

                return _active.Remove(sourceFormation);
            }
            catch
            {
                return false;
            }
        }

        public bool TryGetSequence(Formation sourceFormation, out CavalryChargeSequenceState state)
        {
            state = null;
            if (sourceFormation == null)
            {
                return false;
            }

            return _active.TryGetValue(sourceFormation, out state);
        }

        public void CleanupInvalidSequences(TaleWorlds.MountAndBlade.Mission mission)
        {
            try
            {
                if (_active.Count == 0)
                {
                    return;
                }

                var dead = new List<Formation>();
                foreach (KeyValuePair<Formation, CavalryChargeSequenceState> kv in _active)
                {
                    Formation f = kv.Key;
                    if (!IsFormationSequenceAlive(mission, f))
                    {
                        dead.Add(f);
                    }
                }

                for (int i = 0; i < dead.Count; i++)
                {
                    _active.Remove(dead[i]);
                }
            }
            catch
            {
                // never throw outward
            }
        }

        public void Clear()
        {
            try
            {
                _active.Clear();
            }
            catch
            {
                // ignore
            }
        }

        public void ForEachActive(Action<Formation, CavalryChargeSequenceState> visitor)
        {
            if (visitor == null || _active.Count == 0)
            {
                return;
            }

            try
            {
                KeyValuePair<Formation, CavalryChargeSequenceState>[] snapshot = new KeyValuePair<Formation, CavalryChargeSequenceState>[_active.Count];
                int i = 0;
                foreach (KeyValuePair<Formation, CavalryChargeSequenceState> kv in _active)
                {
                    snapshot[i++] = kv;
                }

                for (int j = 0; j < snapshot.Length; j++)
                {
                    visitor(snapshot[j].Key, snapshot[j].Value);
                }
            }
            catch
            {
                // ignore
            }
        }

        private static bool IsFormationSequenceAlive(TaleWorlds.MountAndBlade.Mission mission, Formation formation)
        {
            if (formation == null || mission?.PlayerTeam == null)
            {
                return false;
            }

            try
            {
                if (formation.CountOfUnits <= 0)
                {
                    return false;
                }

                foreach (Formation f in mission.PlayerTeam.FormationsIncludingEmpty)
                {
                    if (ReferenceEquals(f, formation))
                    {
                        return true;
                    }
                }

                return false;
            }
            catch
            {
                return false;
            }
        }
    }
}
