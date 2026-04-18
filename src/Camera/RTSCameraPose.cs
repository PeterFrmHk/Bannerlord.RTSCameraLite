using TaleWorlds.Library;

namespace Bannerlord.RTSCameraLite.Camera
{
    /// <summary>
    /// Snapshot of desired RTS camera pose (Slice 3: data only; bridge applies later).
    /// </summary>
    public readonly struct RTSCameraPose
    {
        public Vec3 Position { get; }
        public float Yaw { get; }
        public float Pitch { get; }
        public float Height { get; }

        public RTSCameraPose(Vec3 position, float yaw, float pitch, float height)
        {
            Position = position;
            Yaw = yaw;
            Pitch = pitch;
            Height = height;
        }
    }
}
