namespace Bannerlord.RTSCameraLite.Input
{
    /// <summary>
    /// Frame input for RTS free camera (Slice 4); read-only snapshot from <see cref="RTSCameraInput"/>.
    /// </summary>
    internal readonly struct InputSnapshot
    {
        public bool Forward { get; }
        public bool Back { get; }
        public bool Left { get; }
        public bool Right { get; }
        public bool RotateLeft { get; }
        public bool RotateRight { get; }
        public bool FastMove { get; }

        public float ZoomDelta { get; }

        public bool NextFormation { get; }

        public bool PreviousFormation { get; }

        public bool FocusSelectedFormation { get; }

        public InputSnapshot(
            bool forward,
            bool back,
            bool left,
            bool right,
            bool rotateLeft,
            bool rotateRight,
            bool fastMove,
            float zoomDelta,
            bool nextFormation,
            bool previousFormation,
            bool focusSelectedFormation)
        {
            Forward = forward;
            Back = back;
            Left = left;
            Right = right;
            RotateLeft = rotateLeft;
            RotateRight = rotateRight;
            FastMove = fastMove;
            ZoomDelta = zoomDelta;
            NextFormation = nextFormation;
            PreviousFormation = previousFormation;
            FocusSelectedFormation = focusSelectedFormation;
        }
    }
}
