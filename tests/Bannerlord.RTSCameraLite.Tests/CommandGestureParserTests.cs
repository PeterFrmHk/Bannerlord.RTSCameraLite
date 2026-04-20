using Bannerlord.RTSCameraLite.Input;
using Xunit;

namespace Bannerlord.RTSCameraLite.Tests
{
    public sealed class CommandGestureParserTests
    {
        [Fact]
        public void TryReadGroundCommandPreview_NullInput_FailsSafely()
        {
            var parser = new CommandGestureParser();

            GroundCommandPreviewRequest request = parser.TryReadGroundCommandPreview(
                null,
                runtimeHooksEnabled: true,
                formationSelectionEnabled: true,
                groundPreviewEnabled: true,
                commanderModeEnabled: true);

            Assert.False(request.Requested);
        }

        [Fact]
        public void TryReadGroundCommandPreview_DisabledGate_DoesNotRequest()
        {
            var parser = new CommandGestureParser();

            GroundCommandPreviewRequest request = parser.TryReadGroundCommandPreview(
                null,
                runtimeHooksEnabled: true,
                formationSelectionEnabled: true,
                groundPreviewEnabled: false,
                commanderModeEnabled: true);

            Assert.False(request.Requested);
        }

        [Fact]
        public void TryReadGroundCommandPreview_CommanderModeDisabled_DoesNotRequest()
        {
            var parser = new CommandGestureParser();

            GroundCommandPreviewRequest request = parser.TryReadGroundCommandPreview(
                null,
                runtimeHooksEnabled: true,
                formationSelectionEnabled: true,
                groundPreviewEnabled: true,
                commanderModeEnabled: false);

            Assert.False(request.Requested);
        }
    }
}
