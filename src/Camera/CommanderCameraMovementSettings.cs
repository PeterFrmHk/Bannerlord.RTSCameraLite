using System;
using Bannerlord.RTSCameraLite.Config;

namespace Bannerlord.RTSCameraLite.Camera
{
    /// <summary>
    /// Clamped camera tuning derived from <see cref="CommanderConfig"/> (Slice 6).
    /// </summary>
    public sealed class CommanderCameraMovementSettings
    {
        private CommanderCameraMovementSettings(
            float moveSpeed,
            float fastMoveMultiplier,
            float rotationSpeedDegrees,
            float zoomSpeed,
            float defaultHeight,
            float minHeight,
            float maxHeight,
            float defaultPitch)
        {
            MoveSpeed = moveSpeed;
            FastMoveMultiplier = fastMoveMultiplier;
            RotationSpeedDegrees = rotationSpeedDegrees;
            ZoomSpeed = zoomSpeed;
            DefaultHeight = defaultHeight;
            MinHeight = minHeight;
            MaxHeight = maxHeight;
            DefaultPitch = defaultPitch;
        }

        public float MoveSpeed { get; }

        public float FastMoveMultiplier { get; }

        public float RotationSpeedDegrees { get; }

        public float ZoomSpeed { get; }

        public float DefaultHeight { get; }

        public float MinHeight { get; }

        public float MaxHeight { get; }

        public float DefaultPitch { get; }

        public static CommanderCameraMovementSettings FromConfig(CommanderConfig config)
        {
            CommanderConfig baseline = config ?? CommanderConfigDefaults.CreateDefault();

            float minHeight = Math.Max(2f, baseline.MinHeight);
            float maxHeight = baseline.MaxHeight > minHeight ? baseline.MaxHeight : minHeight + 1f;
            float moveSpeed = baseline.MoveSpeed > 0f ? baseline.MoveSpeed : CommanderConfigDefaults.CreateDefault().MoveSpeed;
            float fastMul = baseline.FastMoveMultiplier >= 1f
                ? baseline.FastMoveMultiplier
                : CommanderConfigDefaults.CreateDefault().FastMoveMultiplier;
            float rot = baseline.RotationSpeedDegrees > 0f
                ? baseline.RotationSpeedDegrees
                : CommanderConfigDefaults.CreateDefault().RotationSpeedDegrees;

            float defaultHeight = Math.Max(minHeight, Math.Min(maxHeight, baseline.DefaultHeight));
            float zoomSpeed = baseline.ZoomSpeed;

            return new CommanderCameraMovementSettings(
                moveSpeed,
                fastMul,
                rot,
                zoomSpeed,
                defaultHeight,
                minHeight,
                maxHeight,
                baseline.DefaultPitch);
        }
    }
}
