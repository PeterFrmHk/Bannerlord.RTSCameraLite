using System;
using Bannerlord.RTSCameraLite.Adapters;
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
            Assert.False(d.EnableHarmonyPatches);
            Assert.False(d.EnableHarmonyDiagnostics);
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
            Assert.False(d.EnableCommandValidationDebug);
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

        [Fact]
        public void RuntimeHookGate_Parse_NullOrWhitespace_IsFalse()
        {
            Assert.False(CommanderRuntimeHookGate.TryParseEnableMissionRuntimeHooks(null));
            Assert.False(CommanderRuntimeHookGate.TryParseEnableMissionRuntimeHooks(""));
            Assert.False(CommanderRuntimeHookGate.TryParseEnableMissionRuntimeHooks("   "));
        }

        [Fact]
        public void RuntimeHookGate_Parse_JsonWithoutHookKey_IsFalse()
        {
            // Valid-looking JSON but no EnableMissionRuntimeHooks line => fail closed (no opt-in)
            Assert.False(CommanderRuntimeHookGate.TryParseEnableMissionRuntimeHooks(
                "{\"StartBattlesInCommanderMode\": false, \"ConfigFileVersion\": 1}"));
        }

        [Fact]
        public void TryReadMissionRuntimeHooksEnabledFailClosed_DoesNotThrow()
        {
            Exception caught = null;
            try
            {
                CommanderConfigService.TryReadMissionRuntimeHooksEnabledFailClosed();
            }
            catch (Exception ex)
            {
                caught = ex;
            }

            Assert.Null(caught);
        }

        [Fact]
        public void HarmonyPatchService_UnpatchAll_WithoutApply_DoesNotThrow()
        {
            var s = new HarmonyPatchService();
            s.UnpatchAll();
            s.UnpatchAll();
        }
    }
}
