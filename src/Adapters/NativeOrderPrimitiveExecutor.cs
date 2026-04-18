using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace Bannerlord.RTSCameraLite.Adapters
{
    /// <summary>
    /// Single choke point for native order primitives. Slice 3: returns <see cref="NativeOrderResult.NotWired"/> until
    /// Slice 0 research + in-game verification for <c>OrderController</c> / selection restore is complete.
    /// </summary>
    public sealed class NativeOrderPrimitiveExecutor
    {
        private const string NotWiredReason =
            "Slice 3 skeleton: no OrderController.SetOrder calls until executor is verified against pinned TaleWorlds assemblies (see docs/research/base-game-order-scan.md).";

        public NativeOrderResult ExecuteAdvanceOrMove(Formation formation, Vec3 targetPosition)
        {
            _ = formation;
            _ = targetPosition;
            return NativeOrderResult.NotWired(nameof(ExecuteAdvanceOrMove), NotWiredReason);
        }

        public NativeOrderResult ExecuteCharge(Formation formation)
        {
            _ = formation;
            return NativeOrderResult.NotWired(nameof(ExecuteCharge), NotWiredReason);
        }

        public NativeOrderResult ExecuteHoldOrReform(Formation formation, Vec3 reformPosition)
        {
            _ = formation;
            _ = reformPosition;
            return NativeOrderResult.NotWired(nameof(ExecuteHoldOrReform), NotWiredReason);
        }
    }
}
