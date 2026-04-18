using TaleWorlds.Library;

namespace Bannerlord.RTSCameraLite.Camera
{
    /// <summary>
    /// Commander RTS camera pose (position + orientation + logical height).
    /// </summary>
    public struct CommanderCameraPose
    {
        public Vec3 Position;

        /// <summary>Horizontal rotation in radians (Slice 4 movement plane).</summary>
        public float Yaw;

        /// <summary>Pitch in degrees; Slice 4 keeps this fixed after initialization.</summary>
        public float Pitch;

        /// <summary>Logical camera height / zoom; clamped by <see cref="CommanderCameraMovementSettings"/>.</summary>
        public float Height;
    }
}
