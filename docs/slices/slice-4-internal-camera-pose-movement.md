# Slice 4 — Internal RTS Commander camera pose movement

## Purpose

Deliver **internal** commander camera pose updates (pan, yaw, zoom height) while **Commander Mode** is active, without wiring TaleWorlds **camera application** yet. The mod maintains a coherent `CommanderCameraPose`, exercises `CameraBridge.TryApply` every tick as a **safe choke point**, and keeps **input polling centralized** in `CommanderInputReader`.

## Design audit (minimal)

| | Statement |
| --- | --- |
| **A** | Wire real RTS camera movement immediately. |
| **¬A** | Real camera application is version-sensitive and must stay isolated until Slice 0 / research confirms APIs (`MissionScreen`, frame hooks, ordering). |
| **A\*** | Move only the **internal** `CommanderCameraPose` first, then pass it through **`CameraBridge.TryApply`** as the single adapter-shaped call (still **NotWired** until a later slice). |

## Inputs

| Source | Role |
| --- | --- |
| **`CommanderInputReader.ReadCameraSnapshot`** | W/S/A/D, Q/E, Shift (fast), R/F zoom; all `IInputContext` reads for these keys live here. |
| **`CommanderConfig`** (optional file) | Movement tuning and `StartBattlesInCommanderMode` (Slice 6); defaults match engine literals in `CommanderCameraMovementSettings`. |
| **`CommanderModeState.IsEnabled`** | When **false**, internal camera **Tick** does not run. |
| **`dt`** (`OnMissionTick`) | Frame delta for speed integration. |

## Outputs

| Output | Behavior |
| --- | --- |
| **`CommanderCameraPose`** | Updated **Position** (planar + `z = groundAnchor + Height`), **Yaw** (radians), **Pitch** (degrees, unchanged after init), **Height** (clamped zoom). |
| **`CameraBridge.TryApply`** | Invoked each commander tick with the latest pose; returns **`NotWired`** until a future slice wires engine APIs. |
| **Logs** | One **debug** line the first time a valid internal pose is active; one **warn** line per mission if the bridge never applies (no per-frame spam). |

## Files changed (Slice 4)

| Path | Change |
| --- | --- |
| `src/Input/CommanderInputSnapshot.cs` | **New** — struct holding camera movement booleans + `ZoomDelta`. |
| `src/Input/CommanderInputReader.cs` | **`ReadCameraSnapshot`** using configured/default bindings. |
| `src/Camera/CommanderCameraMovementSettings.cs` | **Engine default constants** + `CreateEngineDefaults()`; `FromConfig` clamp/fallback rules. |
| `src/Camera/CommanderCameraController.cs` | **`Tick(CommanderInputSnapshot, float)`**; ground anchor + yaw-relative planar move; height clamp; `TaleWorlds.MountAndBlade.Mission` qualification. |
| `src/Camera/CommanderCameraPose.cs` | XML clarifying **Yaw** (radians) vs **Pitch** (degrees). |
| `src/Mission/CommanderMissionView.cs` | Commander tick: snapshot → controller → bridge; throttled logging flags. |
| `src/Core/ModLogger.cs` | **`Warn`** (debug sink, no UI spam). |
| `src/Adapters/CameraBridge.cs` | Comment refresh (still **NotWired**). |
| `Bannerlord.RTSCameraLite.csproj` | Compile **`CommanderInputSnapshot.cs`**; informational version. |
| `docs/tests/manual-test-checklist-slice-4.md` | Manual verification steps. |

## Non-goals

- No real **Bannerlord camera** apply / restore (see Slice 5 hard gate).
- No **Harmony**, no **native orders**, no **commander doctrine**, no **formation** automation.
- No **new** config schema for Slice 4 beyond existing optional JSON (Slice 6).
- No suppression of **native player movement**; no terrain raycast for ground follow.

## Test plan

1. `dotnet build -c Release` — zero errors.
2. Custom battle with module enabled; mission supported by `CommanderMissionModeGate`.
3. If `StartBattlesInCommanderMode` is true, commander mode starts **enabled**; otherwise toggle with **Backspace** (or F10 fallback if enabled in config).
4. With commander **enabled**: hold W/S/A/D — planar pose changes; Q/E — yaw; R/F — height clamped between min/max; Shift increases pan speed.
5. With **`CameraBridge` not wired**: no crash; **at most one** bridge warning per mission; no per-frame log flood.
6. Toggle commander **off**: movement stops; toggle **on** again: pose re-seeds from `MainAgent`.
7. Exit battle / mission end: cleanup runs; next mission resets one-shot logs.

## Audit

| Risk | Mitigation |
| --- | --- |
| **Input vs native controls** | Same keys as many native binds; acceptable for this slice — suppression is explicitly out of scope. |
| **Ground anchor fixed at spawn** | `Position.z` uses spawn `Z` + `Height`; no live terrain sampling — documented non-goal. |
| **Yaw convention** | Uses `sin/cos` on horizontal plane consistent with internal-only pose until engine camera alignment. |
| **Config / defaults drift** | `CommanderCameraMovementSettings` exposes **const** defaults aligned with `CommanderConfigDefaults`. |

## See also

- [`manual-test-checklist-slice-4.md`](../tests/manual-test-checklist-slice-4.md)
- Slice 3 adapters: [`slice-3-adapter-boundaries.md`](slice-3-adapter-boundaries.md)
- Research: `docs/research/base-game-camera-scan.md`
