using System;
using Bannerlord.RTSCameraLite.Commands;
using Bannerlord.RTSCameraLite.Core;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace Bannerlord.RTSCameraLite.UX
{
    /// <summary>
    /// Minimal positional command marker: optional one-shot particle burst (public mission API) plus throttled text fallback.
    /// All optional visual uncertainty stays here; failures never propagate to callers.
    /// </summary>
    internal sealed class CommandMarkerService
    {
        /// <summary>
        /// Optional burst name for <see cref="TaleWorlds.MountAndBlade.Mission.AddParticleSystemBurstByName"/>.
        /// Leave null to skip the burst path until verified for your build (see <c>docs/slice-13-audit.md</c>).
        /// </summary>
        private const string OptionalMoveMarkerParticleName = null;

        private readonly TacticalFeedbackService _tactical;
        private readonly Func<TaleWorlds.MountAndBlade.Mission> _getMission;

        private CommandMarkerState _marker;

        public CommandMarkerService(
            TacticalFeedbackService tactical,
            Func<TaleWorlds.MountAndBlade.Mission> getMission)
        {
            _tactical = tactical ?? throw new ArgumentNullException(nameof(tactical));
            _getMission = getMission ?? throw new ArgumentNullException(nameof(getMission));
        }

        /// <summary>
        /// Replaces any active marker with a new one at <paramref name="position"/>.
        /// </summary>
        public void AddMarker(Vec3 position, CommandType commandType, string label)
        {
            try
            {
                _marker = new CommandMarkerState(
                    position,
                    commandType,
                    label,
                    MarkerLifetime.DefaultSeconds);

                TaleWorlds.MountAndBlade.Mission mission = _getMission();
                bool burstAttempted = false;
                bool burstOk = false;
                if (!string.IsNullOrEmpty(OptionalMoveMarkerParticleName)
                    && mission != null
                    && !mission.MissionEnded)
                {
                    burstAttempted = true;
                    burstOk = TryParticleBurst(mission, position);
                }

                if (!burstAttempted || !burstOk)
                {
                    _tactical.ShowPositionalMarkerFallback(position, commandType, label);
                }
            }
            catch (Exception ex)
            {
                ModLogger.LogDebug($"CommandMarkerService.AddMarker swallowed: {ex.Message}");
            }
        }

        /// <summary>
        /// Advances marker lifetime; must run on the mission tick.
        /// </summary>
        public void Tick(float dt)
        {
            try
            {
                if (_marker == null || !_marker.Active)
                {
                    return;
                }

                float next = _marker.RemainingSeconds - dt;
                if (next <= 0f)
                {
                    _marker.RemainingSeconds = 0f;
                    _marker.Active = false;
                    _marker = null;
                    return;
                }

                _marker.RemainingSeconds = next;
            }
            catch (Exception ex)
            {
                ModLogger.LogDebug($"CommandMarkerService.Tick swallowed: {ex.Message}");
            }
        }

        public void Clear()
        {
            _marker = null;
        }

        private static bool TryParticleBurst(TaleWorlds.MountAndBlade.Mission mission, Vec3 position)
        {
            try
            {
                if (mission == null)
                {
                    return false;
                }

                Mat3 rot = Mat3.Identity;
                MatrixFrame frame = new MatrixFrame(rot, position);
                mission.AddParticleSystemBurstByName(OptionalMoveMarkerParticleName, frame, false);
                return true;
            }
            catch (Exception ex)
            {
                ModLogger.LogDebug($"Marker particle burst skipped: {ex.Message}");
                return false;
            }
        }
    }
}
