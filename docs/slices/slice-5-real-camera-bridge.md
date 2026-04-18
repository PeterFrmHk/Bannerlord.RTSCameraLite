# Slice 5 — Real commander camera bridge + restore

**Status:** **NOT BLOCKED** — Slice 0 research identifies a **public** apply path (`MissionScreen.CombatCamera.Frame`) and fallbacks (`Mission.SetCameraFrame` / `GetCameraFrame`, `ResetFirstThirdPersonView`) without Harmony.

## Purpose

Wire **`CameraBridge`** to the **local, verified** Bannerlord APIs from [`docs/research/base-game-camera-scan.md`](../research/base-game-camera-scan.md) and [`docs/research/implementation-decision-slice0.md`](../research/implementation-decision-slice0.md), and restore native camera when **Commander Mode** turns off or the mission UI ends—without scattering engine calls outside the adapter.

## Slice 0 research references

- **Primary apply:** `TaleWorlds.MountAndBlade.View.Screens.MissionScreen.CombatCamera` → `TaleWorlds.Engine.Camera.Frame` (`MatrixFrame` get/set).
- **Fallback apply:** `TaleWorlds.MountAndBlade.Mission.SetCameraFrame(ref MatrixFrame, float)` after snapshot from `GetCameraFrame()` when `CombatCamera` is unavailable.
- **Restore:** re-apply saved `CombatCamera.Frame` and/or saved mission frame via `SetCameraFrame`, then `Mission.ResetFirstThirdPersonView()`.
- **Mission view hook:** `MissionView.UpdateOverridenCamera(float)` — return **`false`** when commander mode is off so vanilla can own the camera (delegates to `base.UpdateOverridenCamera` in that case).
- **Harmony:** **Not used** — Slice 0 / implementation decision: **avoid** for camera on public surfaces.

## Camera API path selected

| Step | Type | Member | Signature (conceptual) |
| --- | --- | --- | --- |
| Bind screen | `MissionScreen` | *(property on `MissionView`)* | `MissionScreen` set from `OnMissionScreenInitialize` into `CameraBridge.SetMissionScreen`. |
| Apply (preferred) | `MissionScreen` | `CombatCamera` | `EngineCamera get_CombatCamera()` |
| Apply (preferred) | `TaleWorlds.Engine.Camera` | `Frame` | `MatrixFrame` get/set |
| Apply (fallback) | `Mission` | `GetCameraFrame` | `MatrixFrame GetCameraFrame()` |
| Apply (fallback) | `Mission` | `SetCameraFrame` | `void SetCameraFrame(ref MatrixFrame, float)` |
| Restore | `Mission` | `ResetFirstThirdPersonView` | `void ResetFirstThirdPersonView()` |

**Spelling note:** engine hook is **`UpdateOverridenCamera`** (Bannerlord typo **Overriden**).

## Restore strategy

1. On **first successful apply**, snapshot **`CombatCamera.Frame`** (if combat camera exists) and/or **`Mission.GetCameraFrame()`** before overwriting.
2. On **commander disable** (Backspace toggle off), **`RestoreNativeCamera`**: write snapshots back, call **`ResetFirstThirdPersonView`**, clear flags.
3. On **`OnMissionScreenFinalize`** / **`OnRemoveBehavior`** / mission ended: same restore, then **`CameraBridge.ResetSession()`** (drops `MissionScreen` reference).

**UNCERTAIN:** ordering vs. deployment / death cam / photo mode — log once on apply failure; validate in-game per [`docs/tests/manual-test-checklist-slice-5.md`](../tests/manual-test-checklist-slice-5.md).

## Failure modes

| Symptom | Cause | Mitigation |
| --- | --- | --- |
| `TryApply` returns not applied | `CombatCamera` null and mission path throws | Throttled **one** debug log per distinct message; `UpdateOverridenCamera` returns `false`. |
| Double speed feel | Native + RTS both writing same frame | Commander off **must** return `base.UpdateOverridenCamera` path. |
| Stuck camera | Restore skipped / exception | `RestoreNativeCamera` fully try/catch; finalize always calls restore + `ResetSession`. |

## Files touched

| File | Role |
| --- | --- |
| `src/Adapters/CameraBridge.cs` | Typed apply/restore + snapshots + `ResetSession`. |
| `src/Adapters/CameraBridgeResult.cs` | `Applied` / `Restored` / `Message` (unchanged shape). |
| `src/Mission/CommanderMissionView.cs` | `UpdateOverridenCamera`, `OnMissionScreenTick` (movement), lifecycle restore. |
| `src/Camera/CommanderCameraController.cs` | WASD/Q/E/R/F movement (hardcoded). |
| `src/Camera/CommanderCameraPose.cs` | Pose struct. |
| `src/Input/CommanderInputReader.cs` | `ReadCameraMovementSnapshot`. |
| `Bannerlord.RTSCameraLite.csproj` | `TaleWorlds.Engine` + `InputSnapshot` compile; trim unrelated adapters for this slice. |

## Non-goals (Slice 5)

- No JSON config, MCM, Harmony, native orders, formation doctrine, Backspace suppression product logic (toggle still uses Backspace as in Slice 2).

## Tests

See [`docs/tests/manual-test-checklist-slice-5.md`](../tests/manual-test-checklist-slice-5.md).

## Audit

| Position | Verdict |
| --- | --- |
| **A** “Apply RTS camera directly inside mission behavior.” | **Rejected** — all engine calls live in **`CameraBridge`** only. |
| **¬A** “APIs are version-sensitive; restore is fragile.” | **Accepted** — documented + manual gate. |
| **A\*** “Application + restore only inside `CameraBridge`.” | **Implemented** — `CommanderMissionView` calls adapter methods only. |

## BLOCKED status

**N/A** for this repository state — if `CombatCamera` is null on a pinned build and fallback fails, treat as **runtime BLOCKER** for that build only and extend research (do not add Harmony without product approval).

## Next slice readiness

- Slice 6+ input ownership / Backspace policy can layer **without** changing `CameraBridge` signatures.
- Re-introduce JSON keys later by feeding `InputSnapshot` from config-driven reader—not by editing `CameraBridge`.
