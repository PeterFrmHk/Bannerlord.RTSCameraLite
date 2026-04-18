namespace Bannerlord.RTSCameraLite.Config
{
    internal static class ConfigDefaults
    {
        public static RTSCameraConfig CreateDefault()
        {
            return new RTSCameraConfig
            {
                ToggleKey = "F10",
                MoveForwardKey = "W",
                MoveBackKey = "S",
                MoveLeftKey = "A",
                MoveRightKey = "D",
                RotateLeftKey = "Q",
                RotateRightKey = "E",
                FastMoveKey = "LeftShift",
                ZoomInKey = "R",
                ZoomOutKey = "F",
                NextFormationKey = "PageDown",
                PreviousFormationKey = "PageUp",
                FocusSelectedFormationKey = "Home",
                MoveSpeed = 12f,
                FastMoveMultiplier = 2.5f,
                RotationSpeedDegrees = 90f,
                ZoomSpeed = 3f,
                DefaultHeight = 18f,
                MinHeight = 6f,
                MaxHeight = 60f,
                DefaultPitch = 60f
            };
        }
    }
}
