using System;
using System.Text;
using Bannerlord.RTSCameraLite.Commander;
using Bannerlord.RTSCameraLite.Config;
using Bannerlord.RTSCameraLite.Doctrine;
using TaleWorlds.MountAndBlade;

namespace Bannerlord.RTSCameraLite.Diagnostics
{
    /// <summary>
    /// Toggle + refresh cadence for portfolio diagnostics (Slice 20 — no gameplay side effects).
    /// </summary>
    public sealed class CommanderDiagnosticsService
    {
        private bool _visible;
        private readonly DiagnosticsThrottle _refreshThrottle = new DiagnosticsThrottle();

        public bool IsVisible => _visible;

        public void ToggleDiagnostics()
        {
            _visible = !_visible;
            if (_visible)
            {
                _refreshThrottle.BumpPastInterval();
            }
        }

        public bool ShouldPublishRefresh(float dt, float intervalSeconds)
        {
            return _visible && _refreshThrottle.TryConsumeRefresh(dt, intervalSeconds);
        }

        /// <summary>No-op hook for symmetry with other mission services.</summary>
        public void Tick(float dt)
        {
        }

        public void Cleanup()
        {
            _visible = false;
            _refreshThrottle.Reset();
        }

        public FormationDiagnosticsSnapshot BuildSnapshot(
            Formation formation,
            CommanderPresenceResult presence,
            CommanderAnchorState anchor,
            FormationDoctrineProfile doctrine,
            FormationEligibilityResult eligibility,
            CommanderRallyState rally,
            CavalryChargeSequenceState doctrineCavalry,
            CavalryChargeSequenceState nativeSequence,
            string targetStateSummary,
            CommanderConfig config)
        {
            string label = SafeFormationLabel(formation);
            string cmd = SummarizeCommanderPresence(presence);
            string anch = SummarizeAnchor(anchor);
            float disc = doctrine?.FormationDisciplineScore ?? 0f;
            string role = doctrine?.Composition?.DominantRole.ToString() ?? "?";
            string elig = SummarizeEligibility(eligibility);
            int total = rally?.TotalTroops ?? 0;
            int rallying = rally?.RallyingTroops ?? 0;
            int absorb = rally?.AbsorbableTroops ?? 0;
            int assigned = rally?.AssignedTroops ?? 0;
            string cav = SummarizeCavalry(nativeSequence, doctrineCavalry);
            string tgt = string.IsNullOrEmpty(targetStateSummary) ? "—" : targetStateSummary;
            string native = SummarizeNativeExecutor(config);
            return new FormationDiagnosticsSnapshot(
                label,
                cmd,
                anch,
                disc,
                role,
                elig,
                total,
                rallying,
                absorb,
                assigned,
                cav,
                tgt,
                native,
                DateTime.UtcNow.Ticks);
        }

        private static string SafeFormationLabel(Formation formation)
        {
            if (formation == null)
            {
                return "?";
            }

            try
            {
                return formation.RepresentativeClass.ToString();
            }
            catch
            {
                return "Formation";
            }
        }

        private static string SummarizeCommanderPresence(CommanderPresenceResult presence)
        {
            if (presence == null)
            {
                return "UC";
            }

            if (!presence.HasCommander)
            {
                return "NO";
            }

            return presence.IsCertain ? "YES" : "UNK";
        }

        private static string SummarizeAnchor(CommanderAnchorState anchor)
        {
            if (!anchor.HasAnchor)
            {
                return "NONE";
            }

            return anchor.CommanderInsideAnchorZone ? "OK" : "OUT";
        }

        private static string SummarizeEligibility(FormationEligibilityResult e)
        {
            if (e == null)
            {
                return "Elig?";
            }

            if (!e.Success)
            {
                return "Elig NO";
            }

            var sb = new StringBuilder();
            sb.Append("Allow");
            if (e.AllowedFormationTypes == null || e.AllowedFormationTypes.Count == 0)
            {
                sb.Append(" —");
            }
            else
            {
                int max = Math.Min(4, e.AllowedFormationTypes.Count);
                for (int i = 0; i < max; i++)
                {
                    sb.Append(i == 0 ? " " : "/");
                    sb.Append(ShortEnum(e.AllowedFormationTypes[i].ToString()));
                }
            }

            return sb.ToString();
        }

        private static string ShortEnum(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return "?";
            }

            return name.Length <= 8 ? name : name.Substring(0, 8);
        }

        private static string SummarizeCavalry(
            CavalryChargeSequenceState nativeSequence,
            CavalryChargeSequenceState doctrineCavalry)
        {
            try
            {
                if (nativeSequence != null && !nativeSequence.Aborted)
                {
                    return "Nv:" + nativeSequence.CurrentState;
                }

                if (doctrineCavalry != null)
                {
                    return "Pl:" + doctrineCavalry.CurrentState;
                }
            }
            catch
            {
                return "?";
            }

            return "—";
        }

        private static string SummarizeNativeExecutor(CommanderConfig config)
        {
            if (config == null)
            {
                return "?";
            }

            try
            {
                return $"ex={config.EnableNativeOrderExecution} pr={config.EnableNativePrimitiveOrderExecution}";
            }
            catch
            {
                return "?";
            }
        }
    }
}
