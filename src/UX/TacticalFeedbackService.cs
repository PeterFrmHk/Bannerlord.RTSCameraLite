using Bannerlord.RTSCameraLite.Commands;
using Bannerlord.RTSCameraLite.Core;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace Bannerlord.RTSCameraLite.UX
{
    /// <summary>
    /// Lightweight player-visible messages for RTS mode (no Gauntlet / HUD overlay).
    /// </summary>
    internal sealed class TacticalFeedbackService
    {
        private readonly FeedbackThrottle _throttle;
        private Formation _lastAnnouncedFormation;

        public TacticalFeedbackService(FeedbackThrottle throttle)
        {
            _throttle = throttle ?? new FeedbackThrottle();
        }

        public void ResetSession()
        {
            _throttle.ResetAll();
            _lastAnnouncedFormation = null;
        }

        public void OnRtsEnabled()
        {
            _throttle.ClearKey("camera-bridge");
        }

        public void OnRtsDisabled()
        {
            _lastAnnouncedFormation = null;
            _throttle.ClearKey("camera-bridge");
        }

        /// <summary>
        /// Call before processing next/previous formation input so the same <see cref="Formation"/> instance
        /// can be announced again (e.g. only one non-empty group in the list).
        /// </summary>
        public void InvalidateFormationAnnouncement()
        {
            _lastAnnouncedFormation = null;
        }

        public void ShowModeEnabled(int toggleCount)
        {
            string msg =
                $"RTS camera ON (toggle #{toggleCount}). Formations: PageDown / PageUp. Focus: Home (see config).";
            ModLogger.PlayerMessage(msg, allowUi: true);
        }

        public void ShowModeDisabled(int toggleCount)
        {
            string msg = $"RTS camera OFF (toggle #{toggleCount}). Native controls restored.";
            ModLogger.PlayerMessage(msg, allowUi: true);
        }

        /// <summary>
        /// Announces a formation selection change (deduped by formation instance).
        /// </summary>
        public void ShowFormationSelected(Formation formation, int listIndex, int unitCount, string classLabel)
        {
            if (ReferenceEquals(formation, _lastAnnouncedFormation))
            {
                return;
            }

            if (formation == null)
            {
                if (_lastAnnouncedFormation != null)
                {
                    ModLogger.PlayerMessage("Formation selection cleared.", allowUi: true);
                }

                _lastAnnouncedFormation = null;
                return;
            }

            _lastAnnouncedFormation = formation;
            string label = string.IsNullOrEmpty(classLabel) ? "group" : classLabel;
            string msg = $"Selected formation: slot {listIndex + 1}, {label}, {unitCount} unit(s).";
            ModLogger.PlayerMessage(msg, allowUi: true);
        }

        public void ShowFocusResult(bool success, string detail)
        {
            string key = success ? "focus-ok" : "focus-fail";
            double cooldown = success ? 1.25 : 2.5;
            if (!_throttle.TryAllow(key, cooldown, forceImmediate: false))
            {
                return;
            }

            string msg = success
                ? $"Camera focus: {detail}"
                : $"Camera focus failed: {detail}";
            ModLogger.PlayerMessage(msg, allowUi: true);
        }

        public void ShowWarning(string message, string throttleKey, double cooldownSeconds, bool forceImmediate)
        {
            if (!_throttle.TryAllow(throttleKey, cooldownSeconds, forceImmediate))
            {
                return;
            }

            ModLogger.PlayerMessage(message, allowUi: true);
        }

        public void ShowDebugLine(string message)
        {
            ModLogger.LogDebug(message);
        }

        /// <summary>
        /// TW-1 number-key formation selection feedback. Event-driven and throttled per slot/message.
        /// </summary>
        public void ShowFormationSelectionFeedback(string message, int slot, bool allowUi)
        {
            if (string.IsNullOrWhiteSpace(message))
            {
                return;
            }

            string key = "tw1-select-" + slot + "-" + message;
            if (!_throttle.TryAllow(key, 0.35, forceImmediate: false))
            {
                return;
            }

            ModLogger.PlayerMessage(message, allowUi: allowUi);
        }

        public void ShowGroundCommandPreview(CommandIntent intent, string formationLabel, bool allowUi)
        {
            if (intent == null || !intent.TargetPosition.HasValue)
            {
                return;
            }

            Vec3 p = intent.TargetPosition.Value;
            string label = string.IsNullOrWhiteSpace(formationLabel) ? "Formation" : formationLabel;
            string key = $"tw2-ground-preview-{label}";
            if (!_throttle.TryAllow(key, 0.35, forceImmediate: false))
            {
                return;
            }

            ModLogger.PlayerMessage(
                $"Preview move: {label} -> ({p.x:0.#},{p.y:0.#},{p.z:0.#})",
                allowUi: allowUi);
        }

        public void ShowGroundCommandPreviewWarning(string message, bool allowUi)
        {
            if (string.IsNullOrWhiteSpace(message))
            {
                return;
            }

            if (!_throttle.TryAllow("tw2-ground-preview-warning-" + message, 0.75, forceImmediate: false))
            {
                return;
            }

            ModLogger.PlayerMessage(message, allowUi: allowUi);
        }

        public void ShowGroundMoveExecutionResult(string message, bool allowUi)
        {
            if (string.IsNullOrWhiteSpace(message))
            {
                return;
            }

            if (!_throttle.TryAllow("tw3-ground-move-execution-" + message, 0.75, forceImmediate: false))
            {
                return;
            }

            ModLogger.PlayerMessage(message, allowUi: allowUi);
        }

        /// <summary>
        /// Slice 10: surfaces command validation only (no order execution).
        /// </summary>
        public void ShowCommandValidation(CommandValidationResult result)
        {
            if (result == null)
            {
                ModLogger.PlayerMessage("[Cmd] Validation error (null result).", allowUi: true);
                return;
            }

            string prefix = result.IsValid ? "[Cmd] OK:" : "[Cmd] Rejected:";
            ModLogger.PlayerMessage($"{prefix} {result.Message}", allowUi: true);
        }

        /// <summary>
        /// Slice 12: surfaces native order execution outcome (validation failures are returned as <see cref="CommandExecutionResult"/> with <c>Executed == false</c>).
        /// </summary>
        public void ShowCommandExecutionResult(CommandExecutionResult result)
        {
            if (result == null)
            {
                ModLogger.PlayerMessage("[Cmd] Execution error (null result).", allowUi: true);
                return;
            }

            if (!result.Executed)
            {
                if (!_throttle.TryAllow("cmd-exec-fail", 2.8, forceImmediate: false))
                {
                    return;
                }

                ModLogger.PlayerMessage($"[Cmd] Not executed ({result.Type}): {result.Message}", allowUi: true);
                return;
            }

            ModLogger.PlayerMessage($"[Cmd] Executed {result.Type}: {result.Message}", allowUi: true);
        }

        /// <summary>
        /// Ground target sampler succeeded (throttled to avoid spam while the camera moves).
        /// </summary>
        public void ShowGroundTargetResolved(Vec3 position, string detail)
        {
            if (!_throttle.TryAllow("ground-target-ok", 14.0, forceImmediate: false))
            {
                return;
            }

            string msg =
                $"[Ground] Target OK ({detail}) @ ({position.x:0.#},{position.y:0.#},{position.z:0.#})";
            ModLogger.PlayerMessage(msg, allowUi: true);
        }

        /// <summary>
        /// Ground target sampler failed (throttled for repeated failures).
        /// </summary>
        public void ShowGroundTargetFailed(string message)
        {
            if (!_throttle.TryAllow("ground-target-fail", 3.5, forceImmediate: false))
            {
                return;
            }

            ModLogger.PlayerMessage($"[Ground] {message}", allowUi: true);
        }

        /// <summary>
        /// Slice 13: throttled text when a world marker cannot be shown (or particle path is disabled).
        /// </summary>
        public void ShowPositionalMarkerFallback(Vec3 position, CommandType commandType, string label)
        {
            if (!_throttle.TryAllow("cmd-marker-fallback", 0.55, forceImmediate: false))
            {
                return;
            }

            string suffix = string.IsNullOrEmpty(label) ? string.Empty : $" ({label})";
            ModLogger.PlayerMessage(
                $"[Marker] {commandType}{suffix} @ ({position.x:0.#},{position.y:0.#},{position.z:0.#})",
                allowUi: true);
        }

        /// <summary>
        /// Slice 13: optional notice for non-positional orders (no world pin).
        /// </summary>
        public void ShowNonPositionalCommandMarkerNotice(CommandType commandType)
        {
            string key = "cmd-marker-np-" + commandType;
            if (!_throttle.TryAllow(key, 2.6, forceImmediate: false))
            {
                return;
            }

            ModLogger.PlayerMessage(
                $"[Marker] {commandType} issued (no map pin for this order type).",
                allowUi: true);
        }

        /// <summary>
        /// Slice 19: text fallback when optional world visuals are disabled or fail (throttled per type/source).
        /// </summary>
        public void ShowCommandMarkerFallback(CommandMarkerType type, Vec3 position, string label, string source)
        {
            string key = "slice19-marker-" + type + "-" + (source ?? string.Empty);
            if (!_throttle.TryAllow(key, 0.35, forceImmediate: false))
            {
                return;
            }

            string lab = string.IsNullOrEmpty(label) ? string.Empty : $" {label}";
            string src = string.IsNullOrEmpty(source) ? string.Empty : $" [{source}]";
            bool hasPosition = !(float.IsNaN(position.x) || float.IsInfinity(position.x)
                || float.IsNaN(position.y) || float.IsInfinity(position.y)
                || float.IsNaN(position.z) || float.IsInfinity(position.z))
                && (position.x * position.x + position.y * position.y + position.z * position.z > 0.0001f);

            string body = hasPosition
                ? $"@ ({position.x:0.#},{position.y:0.#},{position.z:0.#})"
                : "(no position)";
            ModLogger.PlayerMessage($"[Marker] {type}{lab} {body}{src}", allowUi: true);
        }

        /// <summary>
        /// Slice 20 — compact formation / doctrine diagnostics (throttled; no overlay dependency).
        /// </summary>
        public void ShowDiagnosticsSummary(string message, double cooldownSeconds, bool forceImmediate)
        {
            if (string.IsNullOrEmpty(message))
            {
                return;
            }

            if (!_throttle.TryAllow("diagnostics-summary", cooldownSeconds, forceImmediate))
            {
                return;
            }

            ModLogger.PlayerMessage("[Diag] " + message, allowUi: true);
        }
    }
}
