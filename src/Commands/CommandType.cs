namespace Bannerlord.RTSCameraLite.Commands
{
    /// <summary>High-level RTS commander command kinds (Slice 15 — validation / routing).</summary>
    public enum CommandType
    {
        None = 0,
        BasicHold = 1,
        BasicFollow = 2,
        BasicLine = 3,
        Loose = 4,
        ShieldWall = 5,
        Square = 6,
        Circle = 7,
        MountedWide = 8,
        HorseArcherLoose = 9,
        AdvanceOrMove = 10,
        Charge = 11,
        Reform = 12,
        Stop = 13,
        NativeCavalryChargeSequence = 14
    }
}
