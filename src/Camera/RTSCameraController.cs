using System;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using Bannerlord.RTSCameraLite.Input;

namespace Bannerlord.RTSCameraLite.Camera
{
    internal sealed class RTSCameraController
    {
        public const float MoveSpeed = 12f;
        public const float FastMoveMultiplier = 2.5f;
        public const float RotationSpeedDegrees = 90f;
        public const float ZoomSpeed = 3f;
        public const float MinHeight = 6f;
        public const float MaxHeight = 60f;

        private const float DefaultHeight = 18f;
        private const float DefaultPitch = 60f;

        private RTSCameraPose _pose = new RTSCameraPose();
        private float _anchorZ;
        private bool _poseFromAgentSeeded;

        public bool HasSeededPose => _poseFromAgentSeeded;

        public void InitializeFromAgent(Agent agent)
        {
            if (_poseFromAgentSeeded)
            {
                return;
            }

            if (agent == null)
            {
                return;
            }

            Vec3 basePosition = agent.Position;
            _anchorZ = basePosition.z;

            _pose = new RTSCameraPose
            {
                Position = new Vec3(basePosition.x, basePosition.y, _anchorZ + DefaultHeight),
                Height = DefaultHeight,
                Pitch = DefaultPitch,
                Yaw = 0f
            };
            _poseFromAgentSeeded = true;
        }

        public void Tick(InputSnapshot input, float dt)
        {
            if (!_poseFromAgentSeeded || dt <= 0f)
            {
                return;
            }

            float turn = (input.RotateRight ? 1f : 0f) - (input.RotateLeft ? 1f : 0f);
            float yawRad = RotationSpeedDegrees * ((float)Math.PI / 180f) * dt;
            _pose.Yaw += turn * yawRad;

            float move = MoveSpeed * dt * (input.FastMove ? FastMoveMultiplier : 1f);
            float sinY = (float)Math.Sin(_pose.Yaw);
            float cosY = (float)Math.Cos(_pose.Yaw);
            Vec3 forward = new Vec3(sinY, cosY, 0f);
            Vec3 right = new Vec3(cosY, -sinY, 0f);

            float dx = 0f;
            float dy = 0f;
            if (input.Forward)
            {
                dx += forward.x * move;
                dy += forward.y * move;
            }

            if (input.Back)
            {
                dx -= forward.x * move;
                dy -= forward.y * move;
            }

            if (input.Right)
            {
                dx += right.x * move;
                dy += right.y * move;
            }

            if (input.Left)
            {
                dx -= right.x * move;
                dy -= right.y * move;
            }

            float nx = _pose.Position.x + dx;
            float ny = _pose.Position.y + dy;

            float heightDelta = input.ZoomDelta * ZoomSpeed * dt;
            _pose.Height = Clamp(_pose.Height + heightDelta, MinHeight, MaxHeight);
            _pose.Position = new Vec3(nx, ny, _anchorZ + _pose.Height);
        }

        public RTSCameraPose GetPose()
        {
            return _pose;
        }

        public void Reset()
        {
            _poseFromAgentSeeded = false;
            _anchorZ = 0f;
            _pose = new RTSCameraPose();
        }

        private static float Clamp(float value, float min, float max)
        {
            if (value < min)
            {
                return min;
            }

            if (value > max)
            {
                return max;
            }

            return value;
        }
    }
}
