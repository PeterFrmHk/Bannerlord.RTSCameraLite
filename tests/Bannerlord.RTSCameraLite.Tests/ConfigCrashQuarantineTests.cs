using Bannerlord.RTSCameraLite.Config;
using Xunit;

namespace Bannerlord.RTSCameraLite.Tests
{
    public sealed class ConfigCrashQuarantineTests
    {
        [Fact]
        public void CreateDefault_MissionRuntimeAndDormantFlags_AreOff()
        {
            CommanderConfig d = CommanderConfigDefaults.CreateDefault();
            Assert.False(d.EnableMissionRuntimeHooks);
            Assert.False(d.StartBattlesInCommanderMode);
            Assert.False(d.EnableDiagnostics);
            Assert.False(d.EnableCommandMarkers);
            Assert.False(d.EnableCommandRouter);
            Assert.False(d.EnableNativePrimitiveOrderExecution);
            Assert.False(d.EnableNativeOrderExecution);
            Assert.False(d.EnableNativeCavalryChargeSequence);
            Assert.False(d.EnablePerformanceDiagnostics);
            Assert.False(d.WarnOnOverBudget);
            Assert.False(d.EnableDoctrineDebug);
            Assert.False(d.EnableEligibilityDebug);
            Assert.False(d.EnableCommanderAnchorDebug);
            Assert.False(d.EnableRallyAbsorptionDebug);
            Assert.False(d.EnableCavalryDoctrineDebug);
            Assert.False(d.EnableNativeOrderDebug);
            Assert.False(d.EnableCavalrySequenceDebug);
        }

        [Fact]
        public void RuntimeHookGate_Parse_MissingKey_IsFalse()
        {
            Assert.False(CommanderRuntimeHookGate.TryParseEnableMissionRuntimeHooks("{}"));
        }

        [Fact]
        public void RuntimeHookGate_Parse_ExplicitFalse_IsFalse()
        {
            Assert.False(CommanderRuntimeHookGate.TryParseEnableMissionRuntimeHooks(
                "{\"EnableMissionRuntimeHooks\": false}"));
        }

        [Fact]
        public void RuntimeHookGate_Parse_ExplicitTrue_IsTrue()
        {
            Assert.True(CommanderRuntimeHookGate.TryParseEnableMissionRuntimeHooks(
                "{\"EnableMissionRuntimeHooks\": true}"));
        }

        [Fact]
        public void RuntimeHookGate_Parse_InvalidJson_NoMatch_IsFalse()
        {
            Assert.False(CommanderRuntimeHookGate.TryParseEnableMissionRuntimeHooks("{not json"));
        }
    }
}
