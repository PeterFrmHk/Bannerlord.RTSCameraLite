# Manual Test Checklist - TW-2 Ground Command Intent Preview

Status: experimental, opt-in preview only.

## Setup

- Copy the full module build/package output, not only `Bannerlord.RTSCameraLite.dll`.
- Keep `SubModule.xml` at `Modules/Bannerlord.RTSCameraLite/SubModule.xml`.
- Keep config at `Modules/Bannerlord.RTSCameraLite/config/commander_config.json`.
- Set `EnableMissionRuntimeHooks=true` only for this experimental runtime test.
- Set `EnableFormationSelection=true`.
- Set `EnableGroundCommandPreview=true`.
- Keep `EnableNativeOrderExecution=false`.
- Keep `EnableNativePrimitiveOrderExecution=false`.
- Optional: set `EnableCommandMarkers=true` for marker fallback text/visual path testing.

## Main Menu

- Launch to main menu.
- Confirm no crash.
- Confirm TW runtime behavior is not assumed verified from main-menu load alone.

## Battle Load

- Start a custom battle.
- Confirm battle loads without crash.
- Enter commander mode if not already enabled by local test config.

## Preview Flow

- Press `1`, `2`, `3`, or `4` to select a friendly formation.
- Confirm selection feedback appears or debug log records selection.
- Press `G` to preview a ground move under the commander camera ray.
- Confirm marker/text feedback appears, for example `Move preview` or `Preview move: Infantry -> x,y,z`.
- Confirm troops do not move.
- Confirm no native order menu/order execution occurs from the preview.

## Failure Cases

- Press `G` before selecting a formation.
- Confirm safe feedback/log: `No formation selected`.
- Aim camera where ground resolution may fail, if reproducible.
- Confirm safe feedback/log: `No valid ground target`.
- Disable `EnableGroundCommandPreview` and confirm `G` does nothing.

## Acceptance

- No crash on main menu.
- No crash on custom battle load.
- Preview only activates with all gates enabled.
- Selection remains required before preview.
- No command queue appears.
- No enemy targeting occurs.
- No drag-facing occurs.
- No Harmony patches are involved.
- No native order execution occurs.
