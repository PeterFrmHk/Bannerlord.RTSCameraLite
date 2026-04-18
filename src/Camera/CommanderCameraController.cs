using System;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace Bannerlord.RTSCameraLite.Camera
{
    /// <summary>
    /// Commander RTS camera pose state; tuning comes from <see cref="CommanderCameraMovementSettings"/> (Slice 6).
    /// </summary>
    public sealed class CommanderCameraController
    {
        private CommanderCameraPose _pose;
        private bool _hasPose;
        private CommanderCameraMovementSettings _movementSettings = CommanderCameraMovementSettings.FromConfig(null);

        public bool HasPose => _hasPose;

        public CommanderCameraMovementSettings MovementSettings => _movementSettings;

        public void ApplyMovementSettings(CommanderCameraMovementSettings settings)
        {
            _movementSettings = settings ?? CommanderCameraMovementSettings.FromConfig(null);
        }

        public void InitializeFromMission(TaleWorlds.MountAndBlade.Mission mission)
        {
            Reset();
            if (mission?.MainAgent != null)
            {
                InitializeFromAgent(mission.MainAgent);
                return;
            }

            _pose = default;
            _hasPose = false;
        }

        public void InitializeFromAgent(Agent agent)
        {
            if (agent == null)
            {
                _hasPose = false;
                return;
            }

            Vec3 p = agent.Position;
            float height = Math.Max(_movementSettings.MinHeight, Math.Min(_movementSettings.MaxHeight, _movementSettings.DefaultHeight));
            _pose = new CommanderCameraPose
            {
                Position = p,
                Yaw = 0f,
                Pitch = _movementSettings.DefaultPitch,
                Height = height
            };
            _hasPose = true;
        }

        public CommanderCameraPose GetPose()
        {
            return _pose;
        }

        public void Reset()
        {
            _pose = default;
            _hasPose = false;
        }
    }
}
