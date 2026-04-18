using System;
using Bannerlord.RTSCameraLite.Adapters;
using TaleWorlds.MountAndBlade;

namespace Bannerlord.RTSCameraLite.Equipment
{
    /// <summary>
    /// Normalized troop rank / tier estimate (Slice 10).
    /// </summary>
    public readonly struct TroopRankEstimate
    {
        public TroopRankEstimate(float rank01, bool isUncertain)
        {
            Rank01 = rank01;
            IsUncertain = isUncertain;
        }

        /// <summary>0..1 coarse tier.</summary>
        public float Rank01 { get; }

        public bool IsUncertain { get; }
    }

    /// <summary>
    /// Reads rank proxies through <see cref="FormationDataAdapter"/> only.
    /// </summary>
    public static class TroopRankClassifier
    {
        public static TroopRankEstimate EstimateForAgent(FormationDataAdapter adapter, Agent agent)
        {
            if (adapter == null || agent == null)
            {
                return new TroopRankEstimate(0.35f, true);
            }

            try
            {
                FormationDataResult tier = adapter.TryGetAgentTierOrRank(agent);
                if (!tier.Success)
                {
                    return new TroopRankEstimate(0.35f, true);
                }

                float raw = tier.FloatValue;
                float norm = (float)Math.Max(0d, Math.Min(1d, raw / 40d));
                return new TroopRankEstimate(norm, false);
            }
            catch
            {
                return new TroopRankEstimate(0.35f, true);
            }
        }
    }
}
