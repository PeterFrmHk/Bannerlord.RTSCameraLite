namespace Bannerlord.RTSCameraLite.Input
{
    /// <summary>
    /// Per-tick camera movement intent from <see cref="CommanderInputReader.ReadCameraSnapshot"/> (Slice 4).
    /// </summary>
    public readonly struct CommanderInputSnapshot
    {
        public CommanderInputSnapshot(
            bool moveForward,
            bool moveBack,
            bool moveLeft,
            bool moveRight,
            bool rotateLeft,
            bool rotateRight,
            bool fastMove,
            float zoomDelta)
        {
            MoveForward = moveForward;
            MoveBack = moveBack;
            MoveLeft = moveLeft;
            MoveRight = moveRight;
            RotateLeft = rotateLeft;
            RotateRight = rotateRight;
            FastMove = fastMove;
            ZoomDelta = zoomDelta;
        }

        public bool MoveForward { get; }

        public bool MoveBack { get; }

        public bool MoveLeft { get; }

        public bool MoveRight { get; }

        public bool RotateLeft { get; }

        public bool RotateRight { get; }

        public bool FastMove { get; }

        /// <summary>
        /// Zoom axis: negative zooms in (reduces logical height), positive zooms out.
        /// </summary>
        public float ZoomDelta { get; }
    }
}
