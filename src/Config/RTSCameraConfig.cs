namespace Bannerlord.RTSCameraLite.Config
{
    /// <summary>
    /// File-backed RTS camera settings (slice 7). Key fields are string names resolved to engine input keys.
    /// </summary>
    public sealed class RTSCameraConfig
    {
        public string ToggleKey { get; set; }

        public string MoveForwardKey { get; set; }

        public string MoveBackKey { get; set; }

        public string MoveLeftKey { get; set; }

        public string MoveRightKey { get; set; }

        public string RotateLeftKey { get; set; }

        public string RotateRightKey { get; set; }

        public string FastMoveKey { get; set; }

        public string ZoomInKey { get; set; }

        public string ZoomOutKey { get; set; }

        public string NextFormationKey { get; set; }

        public string PreviousFormationKey { get; set; }

        public string FocusSelectedFormationKey { get; set; }

        public float MoveSpeed { get; set; }

        public float FastMoveMultiplier { get; set; }

        public float RotationSpeedDegrees { get; set; }

        public float ZoomSpeed { get; set; }

        public float DefaultHeight { get; set; }

        public float MinHeight { get; set; }

        public float MaxHeight { get; set; }

        public float DefaultPitch { get; set; }
    }
}
