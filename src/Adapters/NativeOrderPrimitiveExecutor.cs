using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace Bannerlord.RTSCameraLite.Adapters
{
    /// <summary>
    /// Single choke point for native order primitives. Slice 3: returns <see cref="NativeOrderResult.CreateNotWired"/> until
    /// Slice 0 research + in-game verification for <c>OrderController</c> / selection restore is complete.
    /// </summary>
    public sealed class NativeOrderPrimitiveExecutor
    {
        private const string NotWiredReason =
            "Slice 3 skeleton: no OrderController.SetOrder calls until executor is verified against pinned TaleWorlds assemblies (see docs/research/base-game-order-scan.md).";

        public NativeOrderResult ExecuteAdvanceOrMove(Formation formation, Vec3 targetPosition)
        {
            return ExecuteAdvanceOrMove(formation, targetPosition, NativeOrderExecutionContext.Default);
        }

        public NativeOrderResult ExecuteAdvanceOrMove(Formation formation, Vec3 targetPosition, NativeOrderExecutionContext context)
        {
            _ = formation;
            _ = targetPosition;
            _ = context;
            return NativeOrderResult.CreateNotWired(nameof(ExecuteAdvanceOrMove), NotWiredReason);
        }

        public NativeOrderResult ExecuteCharge(Formation formation)
        {
            return ExecuteCharge(formation, NativeOrderExecutionContext.Default);
        }

        public NativeOrderResult ExecuteCharge(Formation formation, NativeOrderExecutionContext context)
        {
            _ = formation;
            _ = context;
            return NativeOrderResult.CreateNotWired(nameof(ExecuteCharge), NotWiredReason);
        }

        public NativeOrderResult ExecuteHoldOrReform(Formation formation, Vec3 reformPosition)
        {
            return ExecuteHoldOrReform(formation, reformPosition, NativeOrderExecutionContext.Default);
        }

        public NativeOrderResult ExecuteHoldOrReform(Formation formation, Vec3 reformPosition, NativeOrderExecutionContext context)
        {
            _ = formation;
            _ = reformPosition;
            _ = context;
            return NativeOrderResult.CreateNotWired(nameof(ExecuteHoldOrReform), NotWiredReason);
        }
    }
}
