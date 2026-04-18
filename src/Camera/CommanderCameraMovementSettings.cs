using System;
using Bannerlord.RTSCameraLite.Config;

namespace Bannerlord.RTSCameraLite.Camera
{
    /// <summary>
    /// Tuning for internal commander camera pose movement (Slice 4). Values may originate from
    /// <see cref="CommanderConfig"/> when a file is present (Slice 6); otherwise use <see cref="CreateEngineDefaults"/>.
    /// </summary>
    public sealed class CommanderCameraMovementSettings
    {
        /// <summary>Slice 4 engine defaults (also align with <see cref="CommanderConfigDefaults"/>).</summary>
        public const float DefaultMoveSpeed = 12.0f;

        public const float DefaultFastMoveMultiplier = 2.5f;

        public const float DefaultRotationSpeedDegrees = 90.0f;

        public const float DefaultZoomSpeed = 3.0f;

        public const float DefaultDefaultHeight = 18.0f;

        public const float DefaultMinHeight = 6.0f;

        public const float DefaultMaxHeight = 60.0f;

        public const float DefaultDefaultPitch = 60.0f;

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

        public static CommanderCameraMovementSettings CreateEngineDefaults()
        {
            return new CommanderCameraMovementSettings(
                DefaultMoveSpeed,
                DefaultFastMoveMultiplier,
                DefaultRotationSpeedDegrees,
                DefaultZoomSpeed,
                DefaultDefaultHeight,
                DefaultMinHeight,
                DefaultMaxHeight,
                DefaultDefaultPitch);
        }

        public static CommanderCameraMovementSettings FromConfig(CommanderConfig config)
        {
            CommanderConfig baseline = config ?? CommanderConfigDefaults.CreateDefault();

            float minHeight = Math.Max(2f, baseline.MinHeight);
            float maxHeight = baseline.MaxHeight > minHeight ? baseline.MaxHeight : minHeight + 1f;
            float moveSpeed = baseline.MoveSpeed > 0f ? baseline.MoveSpeed : DefaultMoveSpeed;
            float fastMul = baseline.FastMoveMultiplier >= 1f
                ? baseline.FastMoveMultiplier
                : DefaultFastMoveMultiplier;
            float rot = baseline.RotationSpeedDegrees > 0f
                ? baseline.RotationSpeedDegrees
                : DefaultRotationSpeedDegrees;

            float defaultHeight = Math.Max(minHeight, Math.Min(maxHeight, baseline.DefaultHeight));
            float zoomSpeed = baseline.ZoomSpeed > 0f ? baseline.ZoomSpeed : DefaultZoomSpeed;
            float defaultPitch = baseline.DefaultPitch;

            return new CommanderCameraMovementSettings(
                moveSpeed,
                fastMul,
                rot,
                zoomSpeed,
                defaultHeight,
                minHeight,
                maxHeight,
                defaultPitch);
        }
    }
}
