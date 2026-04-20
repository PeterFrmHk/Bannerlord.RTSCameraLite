# Manual Test Checklist - TW-3 Native Move/Hold Execution

Status: experimental native execution, opt-in only.

## Setup

- Deploy the full module build/package output, not only `Bannerlord.RTSCameraLite.dll`.
- Keep `SubModule.xml` at `Modules/Bannerlord.RTSCameraLite/SubModule.xml`.
- Keep config at `Modules/Bannerlord.RTSCameraLite/config/commander_config.json`.
- First test with default config and confirm no runtime command behavior is active.

## Required Experimental Gates

Set these explicitly for execution testing only:

- `EnableMissionRuntimeHooks=true`
- `EnableFormationSelection=true`
- `EnableGroundCommandPreview=true`
- `EnableGroundMoveExecution=true`
- `EnableCommandRouter=true`
- `EnableNativePrimitiveOrderExecution=true`
- `EnableNativeOrderExecution=true`

Optional:

- `EnableCommandMarkers=true` for marker fallback/visual path testing.

## Preview-Only Baseline

- Set runtime + selection + preview gates true.
- Keep `EnableGroundMoveExecution=false`.
- Start a custom battle.
- Enter commander mode.
- Select a formation with `1`, `2`, `3`, or `4`.
- Press `G`.
- Confirm preview marker/text appears.
- Confirm troops do not move.

## Native Move Execution

- Enable all required experimental gates.
- Start a custom battle.
- Enter commander mode.
- Select one friendly formation with `1`, `2`, `3`, or `4`.
- Aim the commander camera at valid ground.
- Press `Shift+G`.
- Confirm feedback: `Move order issued: [formation label]`.
- Confirm only the selected formation moves toward the resolved ground target.
- Confirm unselected formations do not move.

## Disable Regression

- Set `EnableGroundMoveExecution=false`.
- Repeat `Shift+G`.
- Confirm marker/text preview may appear.
- Confirm no troops move.
- Confirm feedback/log says move order blocked or execution disabled.

## Forbidden Behavior Checks

- Confirm no charge order occurs.
- Confirm no cavalry sequence starts.
- Confirm no enemy targeting occurs.
- Confirm no drag-facing or drag-width behavior occurs.
- Confirm no order queue appears.
- Confirm no Harmony patch behavior is required.

## Acceptance

- Default config cannot move troops.
- Execution requires all explicit gates.
- Failure paths do not crash.
- Main menu load and custom battle load remain separate validation stages.
