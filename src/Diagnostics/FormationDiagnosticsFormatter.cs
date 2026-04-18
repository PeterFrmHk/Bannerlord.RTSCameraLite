using System.Collections.Generic;
using System.Text;

namespace Bannerlord.RTSCameraLite.Diagnostics
{
    /// <summary>
    /// Compact single-line text for screenshots / video overlays (Slice 20).
    /// </summary>
    public static class FormationDiagnosticsFormatter
    {
        /// <summary>
        /// Example style: <c>Infantry: Cmd YES | Disc 0.62 | Allow Line/ShieldWall | Rally 18/32 | Anchor OK</c>
        /// </summary>
        public static string FormatCompactLine(FormationDiagnosticsSnapshot s, DiagnosticsSettings settings)
        {
            if (s == null || settings == null)
            {
                return string.Empty;
            }

            var sb = new StringBuilder(192);
            sb.Append(s.FormationDebugName);
            sb.Append(": Cmd ").Append(s.CommanderPresenceSummary);

            if (settings.IncludeDoctrineScores)
            {
                sb.Append(" | Disc ").Append(s.DoctrineDisciplineScore.ToString("0.00"));
                sb.Append(" | Role ").Append(s.DominantTroopRole);
            }

            if (settings.IncludeEligibility)
            {
                sb.Append(" | ").Append(s.EligibilitySummary);
            }

            if (settings.IncludeRallyAbsorption)
            {
                sb.Append(" | Rally ");
                sb.Append(s.RallyRallying).Append('/').Append(s.RallyTotal);
                sb.Append(" abs ").Append(s.RallyAbsorbable);
                sb.Append(" asg ").Append(s.RallyAssigned);
            }

            sb.Append(" | Anchor ").Append(s.AnchorSummary);

            if (settings.IncludeCavalrySequence)
            {
                sb.Append(" | Cav ").Append(s.CavalrySequenceSummary);
            }

            sb.Append(" | Tgt ").Append(s.TargetStateSummary);

            if (settings.IncludeNativeOrderStatus)
            {
                sb.Append(" | Native ").Append(s.NativeExecutorStatusSummary);
            }

            return sb.ToString();
        }

        public static string FormatMultiFormationBlock(IReadOnlyList<FormationDiagnosticsSnapshot> list, DiagnosticsSettings settings)
        {
            if (list == null || list.Count == 0 || settings == null)
            {
                return string.Empty;
            }

            var sb = new StringBuilder(list.Count * 160);
            for (int i = 0; i < list.Count; i++)
            {
                if (i > 0)
                {
                    sb.Append(" ;; ");
                }

                sb.Append(FormatCompactLine(list[i], settings));
            }

            const int maxLen = 480;
            string full = sb.ToString();
            if (full.Length > maxLen)
            {
                return full.Substring(0, maxLen) + "…";
            }

            return full;
        }
    }
}
