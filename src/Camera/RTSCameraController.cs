using System;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using Bannerlord.RTSCameraLite.Config;
using Bannerlord.RTSCameraLite.Input;

namespace Bannerlord.RTSCameraLite.Camera
{
    internal sealed class RTSCameraController
    {
        private float _moveSpeed = 12f;
        private float _fastMoveMultiplier = 2.5f;
        private float _rotationSpeedDegrees = 90f;
        private float _zoomSpeed = 3f;
        private float _minHeight = 6f;
        private float _maxHeight = 60f;
        private float _defaultHeight = 18f;
        private float _defaultPitch = 60f;

        private RTSCameraPose _pose = new RTSCameraPose();
        private float _anchorZ;
        private bool _poseFromAgentSeeded;

        public bool HasSeededPose => _poseFromAgentSeeded;

        public void ApplyCameraSettings(RTSCameraConfig config)
        {
            RTSCameraConfig source = config ?? ConfigDefaults.CreateDefault();

            _moveSpeed = Clamp(source.MoveSpeed, 0.1f, 200f);
            _fastMoveMultiplier = Clamp(source.FastMoveMultiplier, 1f, 10f);
            _rotationSpeedDegrees = Clamp(source.RotationSpeedDegrees, 1f, 720f);
            _zoomSpeed = Clamp(source.ZoomSpeed, 0f, 50f);

            float minH = Clamp(source.MinHeight, 1f, 500f);
            float maxH = Clamp(source.MaxHeight, 1f, 500f);
            if (minH >= maxH)
            {
                RTSCameraConfig d = ConfigDefaults.CreateDefault();
                minH = d.MinHeight;
                maxH = d.MaxHeight;
            }

            _minHeight = minH;
            _maxHeight = maxH;
            _defaultHeight = Clamp(source.DefaultHeight, _minHeight, _maxHeight);
            _defaultPitch = Clamp(source.DefaultPitch, 0f, 89f);
        }

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
                Position = new Vec3(basePosition.x, basePosition.y, _anchorZ + _defaultHeight),
                Height = _defaultHeight,
                Pitch = _defaultPitch,
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
            float yawRad = _rotationSpeedDegrees * ((float)Math.PI / 180f) * dt;
            _pose.Yaw += turn * yawRad;

            float move = _moveSpeed * dt * (input.FastMove ? _fastMoveMultiplier : 1f);
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

            float heightDelta = input.ZoomDelta * _zoomSpeed * dt;
            _pose.Height = Clamp(_pose.Height + heightDelta, _minHeight, _maxHeight);
            _pose.Position = new Vec3(nx, ny, _anchorZ + _pose.Height);
        }

        public RTSCameraPose GetPose()
        {
            return _pose;
        }

        /// <summary>
        /// Moves the camera origin over the given world point while keeping current height and yaw.
        /// </summary>
        public void FocusAt(Vec3 worldPosition)
        {
            if (!_poseFromAgentSeeded)
            {
                return;
            }

            if (float.IsNaN(worldPosition.x) || float.IsNaN(worldPosition.y) || float.IsNaN(worldPosition.z) ||
                float.IsInfinity(worldPosition.x) || float.IsInfinity(worldPosition.y) || float.IsInfinity(worldPosition.z))
            {
                return;
            }

            _anchorZ = worldPosition.z;
            _pose.Position = new Vec3(worldPosition.x, worldPosition.y, _anchorZ + _pose.Height);
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
