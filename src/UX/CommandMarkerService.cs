using System;
using System.Collections.Generic;
using Bannerlord.RTSCameraLite.Core;
using Bannerlord.RTSCameraLite.Tactical;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace Bannerlord.RTSCameraLite.UX
{
    /// <summary>
    /// Temporary world markers for command targets (Slice 19). Visual path is optional; failures fall back to text and never throw.
    /// </summary>
    internal sealed class CommandMarkerService
    {
        /// <summary>
        /// Optional particle name for <see cref="TaleWorlds.MountAndBlade.Mission.AddParticleSystemBurstByName"/>.
        /// Leave null until Slice 0 research verifies a safe effect name for your build.
        /// </summary>
        private const string OptionalMarkerParticleName = null;

        private const int MaxMarkers = 16;

        private readonly CommandMarkerSettings _settings;
        private readonly TacticalFeedbackService _tactical;
        private readonly Func<TaleWorlds.MountAndBlade.Mission> _getMission;
        private readonly List<CommandMarkerState> _markers = new List<CommandMarkerState>();
        private readonly Dictionary<CommandMarkerType, DateTime> _lastAddUtc = new Dictionary<CommandMarkerType, DateTime>();

        public CommandMarkerService(
            CommandMarkerSettings settings,
            TacticalFeedbackService tactical,
            Func<TaleWorlds.MountAndBlade.Mission> getMission)
        {
            _settings = settings ?? CommandMarkerSettings.FromConfig(null);
            _tactical = tactical;
            _getMission = getMission ?? (() => null);
        }

        public void AddMarker(CommandMarkerType type, Vec3 position, string label, string source)
        {
            try
            {
                if (!_settings.EnableCommandMarkers)
                {
                    return;
                }

                if (!IsFiniteVec3(position))
                {
                    return;
                }

                DateTime now = DateTime.UtcNow;
                if (_lastAddUtc.TryGetValue(type, out DateTime last)
                    && (now - last).TotalSeconds < _settings.MarkerRefreshThrottleSeconds)
                {
                    return;
                }

                _lastAddUtc[type] = now;

                TaleWorlds.MountAndBlade.Mission mission = _getMission();
                MarkerRenderResult render = TryRenderVisual(mission, position, type);
                bool visualOk = render.VisualRendered;

                if (!visualOk && _settings.EnableFallbackTextMarkers && _tactical != null)
                {
                    _tactical.ShowCommandMarkerFallback(type, position, label, source);
                }

                float life = MarkerLifetime.Resolve(type, _settings);
                _markers.Add(
                    new CommandMarkerState(
                        active: true,
                        type: type,
                        position: position,
                        label: label,
                        remainingSeconds: life,
                        source: source ?? string.Empty,
                        visualRendered: visualOk,
                        reason: render.Reason));

                TrimExcessMarkers();
            }
            catch (Exception ex)
            {
                ModLogger.LogDebug($"{ModConstants.ModuleId}: CommandMarkerService.AddMarker suppressed ({ex.Message})");
            }
        }

        public void AddGroundTargetMarker(GroundTargetResult target)
        {
            try
            {
                if (!target.Success)
                {
                    if (_settings.EnableCommandMarkers
                        && _settings.EnableFallbackTextMarkers
                        && _tactical != null)
                    {
                        _tactical.ShowCommandMarkerFallback(
                            CommandMarkerType.GroundTarget,
                            Vec3.Zero,
                            "Ground",
                            "ground-target-failed: " + target.Message);
                    }

                    return;
                }

                AddMarker(CommandMarkerType.GroundTarget, target.Position, "Ground", "ground-target");
            }
            catch (Exception ex)
            {
                ModLogger.LogDebug($"{ModConstants.ModuleId}: AddGroundTargetMarker suppressed ({ex.Message})");
            }
        }

        public void AddChargeTargetMarker(FormationTargetResult target)
        {
            try
            {
                if (!target.Success || !IsFiniteVec3(target.Position))
                {
                    return;
                }

                AddMarker(CommandMarkerType.ChargeTarget, target.Position, "Charge", "cavalry-charge-target");
            }
            catch (Exception ex)
            {
                ModLogger.LogDebug($"{ModConstants.ModuleId}: AddChargeTargetMarker suppressed ({ex.Message})");
            }
        }

        public void AddReformMarker(Vec3 position)
        {
            try
            {
                AddMarker(CommandMarkerType.ReformPoint, position, "Reform", "cavalry-reform");
            }
            catch (Exception ex)
            {
                ModLogger.LogDebug($"{ModConstants.ModuleId}: AddReformMarker suppressed ({ex.Message})");
            }
        }

        public void Tick(float dt)
        {
            try
            {
                if (dt <= 0f || _markers.Count == 0)
                {
                    return;
                }

                for (int i = _markers.Count - 1; i >= 0; i--)
                {
                    CommandMarkerState m = _markers[i];
                    if (!m.Active)
                    {
                        _markers.RemoveAt(i);
                        continue;
                    }

                    float next = m.RemainingSeconds - dt;
                    if (next <= 0f)
                    {
                        m.Active = false;
                        m.RemainingSeconds = 0f;
                        _markers.RemoveAt(i);
                    }
                    else
                    {
                        m.RemainingSeconds = next;
                    }
                }
            }
            catch (Exception ex)
            {
                ModLogger.LogDebug($"{ModConstants.ModuleId}: CommandMarkerService.Tick suppressed ({ex.Message})");
            }
        }

        public void Cleanup()
        {
            try
            {
                _markers.Clear();
                _lastAddUtc.Clear();
            }
            catch
            {
                // ignore
            }
        }

        private void TrimExcessMarkers()
        {
            while (_markers.Count > MaxMarkers)
            {
                _markers.RemoveAt(0);
            }
        }

        private static MarkerRenderResult TryRenderVisual(
            TaleWorlds.MountAndBlade.Mission mission,
            Vec3 position,
            CommandMarkerType type)
        {
            try
            {
                if (string.IsNullOrEmpty(OptionalMarkerParticleName))
                {
                    return MarkerRenderResult.Skipped("no particle name configured (Slice 0)");
                }

                if (mission == null || mission.MissionEnded)
                {
                    return MarkerRenderResult.Skipped("mission unavailable");
                }

                Mat3 rot = Mat3.Identity;
                MatrixFrame frame = new MatrixFrame(rot, position);
                mission.AddParticleSystemBurstByName(OptionalMarkerParticleName, frame, false);
                _ = type;
                return MarkerRenderResult.VisualOk("particle burst issued");
            }
            catch (Exception ex)
            {
                return MarkerRenderResult.Skipped("visual: " + ex.Message);
            }
        }

        private static bool IsFiniteVec3(Vec3 v)
        {
            return !(float.IsNaN(v.x) || float.IsInfinity(v.x)
                || float.IsNaN(v.y) || float.IsInfinity(v.y)
                || float.IsNaN(v.z) || float.IsInfinity(v.z));
        }
    }
}
