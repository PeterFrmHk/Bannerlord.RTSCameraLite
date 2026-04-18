namespace Bannerlord.RTSCameraLite.Commands
{
    /// <summary>
    /// Native engine primitive routed only through <see cref="NativeOrderPrimitiveExecutor"/> (Slice 14).
    /// </summary>
    public enum NativeOrderPrimitive
    {
        None = 0,
        AdvanceOrMove = 1,
        Charge = 2,
        Hold = 3,
        Reform = 4,
        FollowCommander = 5,
        Stop = 6
    }
}
