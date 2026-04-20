using Bannerlord.RTSCameraLite.Selection;
using Xunit;

namespace Bannerlord.RTSCameraLite.Tests
{
    public sealed class FormationSelectionTests
    {
        [Fact]
        public void FormationSelectionState_StartsEmpty()
        {
            var state = new FormationSelectionState();

            Assert.Equal(0, state.SelectedCount);
            Assert.Empty(state.SelectedFormations);
            Assert.Empty(state.SnapshotSelectedFormations());
        }

        [Fact]
        public void FormationSelectionState_SelectSingleNull_FailsClosed()
        {
            var state = new FormationSelectionState();

            Assert.False(state.SelectSingle(null));
            Assert.Equal(0, state.SelectedCount);
        }

        [Fact]
        public void FormationSelectionState_Clear_EmptiesState()
        {
            var state = new FormationSelectionState();

            state.Clear();

            Assert.Equal(0, state.SelectedCount);
            Assert.Empty(state.SnapshotSelectedFormations());
        }

        [Fact]
        public void FormationSelectionState_TryGetPrimaryEmpty_FailsSafely()
        {
            var state = new FormationSelectionState();

            Assert.False(state.TryGetPrimarySelectedFormation(out var formation));
            Assert.Null(formation);
        }

        [Fact]
        public void FormationSelectionService_NullMission_FailsSafely()
        {
            var service = new FormationSelectionService();
            var state = new FormationSelectionState();

            FormationSelectionResult result = service.TrySelectNumberKeySlot(
                null,
                runtimeHooksEnabled: true,
                formationSelectionEnabled: true,
                slot: 1,
                state: state);

            Assert.True(result.Handled);
            Assert.False(result.Success);
            Assert.Equal(0, state.SelectedCount);
        }

        [Fact]
        public void FormationSelectionService_DisabledGate_DoesNotHandle()
        {
            var service = new FormationSelectionService();
            var state = new FormationSelectionState();

            FormationSelectionResult result = service.TrySelectNumberKeySlot(
                null,
                runtimeHooksEnabled: true,
                formationSelectionEnabled: false,
                slot: 1,
                state: state);

            Assert.False(result.Handled);
            Assert.False(result.Success);
            Assert.Equal(0, state.SelectedCount);
        }
    }
}
