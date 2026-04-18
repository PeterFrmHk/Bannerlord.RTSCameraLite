# Slice 8 audit — formation query, selection, and camera focus (RTS only)

## Audit logic (design rationale)

- **A:** Free camera is enough for basic orientation.
- **¬A:** RTS camera without formation awareness is only a flying spectator toy.
- **A\*:** Add formation query and camera focus **before** command routing so behavior stays read-only and testable.

## Goal

While RTS camera mode is enabled, let the player cycle friendly formations and snap the camera anchor to a derived formation position. No orders, no UI overlay, no command routing.

## Behavior

| Area | Implementation |
|------|------------------|
| **Query** | `FormationQueryService` walks `TaleWorlds.Core.FormationClass` values in stable sorted order, calls `Team.GetFormation(FormationClass)` on `MainAgent.Team`, keeps non-null formations with `CountOfUnits > 0`. Result cached ~8 mission ticks. |
| **Selection** | `FormationSelectionState` holds index + `Formation` reference; `NextFormation` / `PreviousFormation` wrap; `ClearIfInvalid` drops stale or empty selections. |
| **Focus** | `FormationFocusController` resolves `Vec3` in order: median agent (`GetMedianAgent`), captain position, then `OrderPosition` (XY) + reference Z from captain or main agent. All steps try/catch guarded. |
| **Camera** | `RTSCameraController.FocusAt` updates `_anchorZ` and horizontal position; **yaw and height unchanged**. |
| **Input** | `NextFormationKey` (default `PageDown`), `PreviousFormationKey` (`PageUp`), `FocusSelectedFormationKey` (default **`Home`**) in JSON; read only while RTS owns input (`RTSCameraInput`). Uses `IsKeyPressed` for one step per press. **`Space` is intentionally not the default** — Bannerlord stacks key bindings; `Home` reduces clashes with native combat/UI. |

## Non-goals (explicit)

- No `IssueOrder`, no AI behavior changes, no formation markers, no pause/slow-motion, no MCM.

## Risks / limitations

- Formation APIs differ slightly across game versions; all calls are wrapped in try/catch at the service layer.
- `PageDown` / `PageUp` / `Home` can still overlap other mods or user binds while RTS is on; everything is overridable in `config/rts_camera_lite.json`.
- If `MainAgent` is null (unusual in supported missions), formation list is empty.

## Manual checks

1. `dotnet build` succeeds.
2. RTS on: `PageDown` / `PageUp` cycles selection log; no crash with empty army edge cases.
3. RTS on: `Home` (or your configured focus key) moves the camera over the selected formation; height/yaw unchanged from before focus.
4. RTS off: formation keys do nothing (input not read).
5. Destroyed / depleted formation: `ClearIfInvalid` clears selection without crash.
