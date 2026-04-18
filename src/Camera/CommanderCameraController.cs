using System;
using Bannerlord.RTSCameraLite.Input;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace Bannerlord.RTSCameraLite.Camera
{
    /// <summary>
    /// Commander RTS camera pose state; internal movement (Slice 4) while <see cref="HasPose"/> is true.
    /// </summary>
    public sealed class CommanderCameraController
    {
        private CommanderCameraPose _pose;
        private bool _hasPose;
        private float _groundAnchorZ;
        private CommanderCameraMovementSettings _movementSettings = CommanderCameraMovementSettings.CreateEngineDefaults();

        public bool HasPose => _hasPose;

        public CommanderCameraMovementSettings MovementSettings => _movementSettings;

        public void ApplyMovementSettings(CommanderCameraMovementSettings settings)
        {
            _movementSettings = settings ?? CommanderCameraMovementSettings.CreateEngineDefaults();
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
            _groundAnchorZ = p.z;
            float height = Math.Max(_movementSettings.MinHeight, Math.Min(_movementSettings.MaxHeight, _movementSettings.DefaultHeight));
            _pose = new CommanderCameraPose
            {
                Position = new Vec3(p.x, p.y, _groundAnchorZ + height),
                Yaw = 0f,
                Pitch = _movementSettings.DefaultPitch,
                Height = height
            };
            _hasPose = true;
        }

        /// <summary>
        /// Advances internal pose from input. Pitch stays fixed; height is clamped; horizontal motion uses <see cref="CommanderCameraPose.Yaw"/>.
        /// </summary>
        public void Tick(CommanderInputSnapshot input, float dt)
        {
            if (!_hasPose || dt <= 0f)
            {
                return;
            }

            CommanderCameraMovementSettings s = _movementSettings;
            float speed = s.MoveSpeed * (input.FastMove ? s.FastMoveMultiplier : 1f) * dt;
            float yaw = _pose.Yaw;
            float rotRad = s.RotationSpeedDegrees * ((float)Math.PI / 180f) * dt;
            if (input.RotateLeft)
            {
                yaw -= rotRad;
            }

            if (input.RotateRight)
            {
                yaw += rotRad;
            }

            float sin = (float)Math.Sin(yaw);
            float cos = (float)Math.Cos(yaw);
            Vec2 forward = new Vec2(sin, cos);
            Vec2 right = new Vec2(cos, -sin);
            Vec2 planar = new Vec2(_pose.Position.x, _pose.Position.y);
            if (input.MoveForward)
            {
                planar += forward * speed;
            }

            if (input.MoveBack)
            {
                planar -= forward * speed;
            }

            if (input.MoveLeft)
            {
                planar -= right * speed;
            }

            if (input.MoveRight)
            {
                planar += right * speed;
            }

            float height = _pose.Height + input.ZoomDelta * s.ZoomSpeed * dt;
            height = Math.Max(s.MinHeight, Math.Min(s.MaxHeight, height));
            _pose.Yaw = yaw;
            _pose.Height = height;
            _pose.Position = new Vec3(planar.x, planar.y, _groundAnchorZ + height);
        }

        public CommanderCameraPose GetPose()
        {
            return _pose;
        }

        public void Reset()
        {
            _pose = default;
            _hasPose = false;
            _groundAnchorZ = 0f;
        }
    }
}
