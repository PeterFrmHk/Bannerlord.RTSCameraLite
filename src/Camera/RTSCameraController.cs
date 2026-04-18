using System;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace Bannerlord.RTSCameraLite.Camera
{
    /// <summary>
    /// Owns RTS pose state; no movement or input yet (Slice 3).
    /// </summary>
    public sealed class RTSCameraController
    {
        private const float DefaultHeight = 18f;
        private const float DefaultPitch = 60f;

        private RTSCameraPose _pose;
        private bool _initialized;

        public bool IsInitialized => _initialized;

        public void InitializeFromAgent(Agent agent)
        {
            if (agent == null)
            {
                return;
            }

            Vec3 position = agent.Position;
            float yaw = ComputeYawFromLook(agent.LookDirection);

            _pose = new RTSCameraPose(position, yaw, DefaultPitch, DefaultHeight);
            _initialized = true;
        }

        public RTSCameraPose GetPose()
        {
            if (!_initialized)
            {
                return new RTSCameraPose(Vec3.Zero, 0f, DefaultPitch, DefaultHeight);
            }

            return _pose;
        }

        public void Reset()
        {
            _initialized = false;
            _pose = default;
        }

        private static float ComputeYawFromLook(Vec3 look)
        {
            if (look.LengthSquared < 1e-6f)
            {
                return 0f;
            }

            look.Normalize();
            return (float)Math.Atan2(look.x, look.y);
        }
    }
}
