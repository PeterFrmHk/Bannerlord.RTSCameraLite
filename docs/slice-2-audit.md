# Slice 2 Audit: RTS toggle state + battle `MissionView`

## Purpose

Attach a **battle-only** `MissionView` that maintains an internal RTS camera **toggle state** (no camera movement, no Harmony, no input blocking). Prove runtime mission attachment and F10-driven state in custom battle.

## Function spec (minimal shape)

- **`RTSCameraState`:** `Enabled`, `ToggleCount`, `LastToggleReason` (default `"Not toggled"`), `Toggle(string reason)`.
- **`MissionModeGate`:** `mission.Mode == Battle || Deployment` only.
- **`RTSCameraMissionBehavior`:** `MissionView`; `OnBehaviorInitialize` logs attach; `OnMissionTick` reads F10, toggles state, logs via `ModLogger.SafeStartupLog`.
- **`SubModule`:** `OnMissionBehaviorInitialize(TaleWorlds.MountAndBlade.Mission mission)` — fully qualified parameter so `using Bannerlord.RTSCameraLite.Mission` does not break the `Mission` type name.

## Implementation

| Area | Detail |
| --- | --- |
| Namespace | **`Bannerlord.RTSCameraLite.Mission`** (types under `src/Mission/`). |
| `MissionView` | `using TaleWorlds.MountAndBlade.View.MissionViews` (class is not in `TaleWorlds.MountAndBlade` root). |
| API spelling | TaleWorlds uses US spelling: **`OnBehaviorInitialize`**, **`OnMissionBehaviorInitialize`** (not `Behaviour`). |
| F10 | **`IsKeyReleased(InputKey.F10)`** instead of `IsKeyPressed`: `IsKeyPressed` is true every tick while the key is held, which would not match enable/disable acceptance. |
| `OnMissionTick` | Used (inherits from `MissionBehavior`); not `OnMissionScreenTick`. |
| Logging | Toggle lines use **`ModLogger.SafeStartupLog`** per minimal shape (attach line + status). |
| References | NuGet metapackage supplies `TaleWorlds.MountAndBlade.View`; game-folder `.csproj` includes `TaleWorlds.MountAndBlade.View.dll`. |

## Tests

- [ ] Build passes
- [ ] Mod appears in launcher
- [ ] Custom battle loads
- [ ] RTS behavior attaches
- [ ] F10 enables state
- [ ] F10 disables state
- [ ] Toggle spam does not crash
- [ ] No camera movement occurs

**Automated:** `dotnet build` — succeeded on verification host (0 warnings).

**Manual:** `docs/manual-test-checklist.md` — Slice 2 section.

## PRISM

- **A:** Start camera movement now.
- **¬A:** Camera movement needs validated mission attachment first.
- **A\*:** Slice 2 only proves runtime mission attachment and toggle state.

## Audit

### Implemented

- `RTSCameraState`, `MissionModeGate`, `RTSCameraMissionBehavior` in `Bannerlord.RTSCameraLite.Mission`.
- `SubModule` registers `RTSCameraMissionBehavior` when gate passes.

### Not implemented

- Camera movement, `UpdateOverridenCamera`, Harmony, MCM, UI, formations, command routing.

### Known risks

- Gate excludes all non-`Battle` / `Deployment` modes; F10 inert until those modes.
- F10 is hard-coded.

### Next slice readiness

Complete all manual tests above before adding camera motion or input routing.
