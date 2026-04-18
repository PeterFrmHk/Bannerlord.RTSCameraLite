using TaleWorlds.Library;

namespace Bannerlord.RTSCameraLite.Camera
{
    /// <summary>
    /// Commander RTS camera pose (position + orientation + logical height).
    /// </summary>
    public struct CommanderCameraPose
    {
        public Vec3 Position;
        public float Yaw;
        public float Pitch;
        public float Height;
    }
}
