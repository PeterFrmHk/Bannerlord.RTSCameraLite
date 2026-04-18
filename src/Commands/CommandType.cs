namespace Bannerlord.RTSCameraLite.Commands
{
    /// <summary>
    /// High-level command kinds for future routing (slice 10: validation only).
    /// </summary>
    public enum CommandType
    {
        None = 0,
        HoldPosition,
        MoveToPosition,
        Charge,
        FollowPlayer,
        FaceDirection,
        Advance,
        Retreat
    }
}
