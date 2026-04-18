namespace Bannerlord.RTSCameraLite.Adapters
{
    /// <summary>
    /// Maps high-level command decisions to <see cref="NativeOrderPrimitiveExecutor"/> entry points (Slice 15).
    /// </summary>
    public enum NativeOrderPrimitive
    {
        None = 0,
        AdvanceOrMove = 1,
        Charge = 2,
        HoldOrReform = 3
    }
}
