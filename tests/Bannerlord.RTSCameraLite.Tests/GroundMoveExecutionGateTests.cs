using Bannerlord.RTSCameraLite.Commands;
using Bannerlord.RTSCameraLite.Config;
using Xunit;

namespace Bannerlord.RTSCameraLite.Tests
{
    public sealed class GroundMoveExecutionGateTests
    {
        [Fact]
        public void CanExecute_DefaultConfig_Blocks()
        {
            CommanderConfig config = CommanderConfigDefaults.CreateDefault();

            bool allowed = GroundMoveExecutionGate.CanExecute(config, nativeExecutorAvailable: true, out string reason);

            Assert.False(allowed);
            Assert.Equal("mission runtime hooks disabled", reason);
        }

        [Fact]
        public void CanExecute_GroundMoveExecutionDisabled_Blocks()
        {
            CommanderConfig config = CommanderConfigDefaults.CreateDefault();
            config.EnableMissionRuntimeHooks = true;
            config.EnableFormationSelection = true;
            config.EnableGroundCommandPreview = true;
            config.EnableCommandRouter = true;
            config.EnableNativePrimitiveOrderExecution = true;
            config.EnableNativeOrderExecution = true;

            bool allowed = GroundMoveExecutionGate.CanExecute(config, nativeExecutorAvailable: true, out string reason);

            Assert.False(allowed);
            Assert.Equal("ground move execution disabled", reason);
        }

        [Fact]
        public void CanExecute_NativeExecutionDisabled_Blocks()
        {
            CommanderConfig config = CommanderConfigDefaults.CreateDefault();
            config.EnableMissionRuntimeHooks = true;
            config.EnableFormationSelection = true;
            config.EnableGroundCommandPreview = true;
            config.EnableGroundMoveExecution = true;
            config.EnableCommandRouter = true;
            config.EnableNativePrimitiveOrderExecution = true;

            bool allowed = GroundMoveExecutionGate.CanExecute(config, nativeExecutorAvailable: true, out string reason);

            Assert.False(allowed);
            Assert.Equal("native order execution disabled", reason);
        }

        [Fact]
        public void CanExecute_AllGatesEnabled_Allows()
        {
            CommanderConfig config = CommanderConfigDefaults.CreateDefault();
            config.EnableMissionRuntimeHooks = true;
            config.EnableFormationSelection = true;
            config.EnableGroundCommandPreview = true;
            config.EnableGroundMoveExecution = true;
            config.EnableCommandRouter = true;
            config.EnableNativePrimitiveOrderExecution = true;
            config.EnableNativeOrderExecution = true;

            bool allowed = GroundMoveExecutionGate.CanExecute(config, nativeExecutorAvailable: true, out string reason);

            Assert.True(allowed);
            Assert.Equal(string.Empty, reason);
        }
    }
}
