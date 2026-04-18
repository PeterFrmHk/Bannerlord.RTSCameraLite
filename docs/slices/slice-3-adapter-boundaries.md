# Slice 3 — Adapter boundaries for version-sensitive Bannerlord systems

## Purpose

Isolate **TaleWorlds / Native** surfaces that drift between game versions behind **small, explicit adapters** so gameplay code (`MissionView`, future doctrine) never sprinkles raw engine calls. Slice 3 ships **skeletons + safe no-op / not-wired results** only.

## Adapter boundaries (what lives where)

| Adapter | Responsibility | Slice 3 wiring |
| --- | --- | --- |
| **`CameraBridge`** (`src/Adapters/CameraBridge.cs`) | All future camera writes/restores (`Mission`, `MissionScreen`, `Camera.Frame`, …). | **`TryApply` / `RestoreNativeCamera` → `CameraBridgeResult.NotWired`** with a fixed reason string. No `MissionView` reference yet — wiring needs `UpdateOverridenCamera` context in a later slice. |
| **`NativeOrderPrimitiveExecutor`** | `OrderController` / `MovementOrder` / selection restore. | **All primitives → `NativeOrderResult.NotWired`**. No `SetOrder` calls until Slice 0 + ILSpy verification is complete for the pinned build. |
| **`FormationDataAdapter`** | Read-only formation probes. | **Partial:** `TryGetFormationCenter` lifts planar **`Vec2`** (`SmoothedAverageUnitPosition` / `CurrentPosition` / `OrderPosition`) with **`OrderGroundPosition.z`** (NuGet 1.2.x differs from 1.3.x `CachedAveragePosition` naming). `TryGetAgents` uses `ApplyActionOnEachUnit`. **Not wired / failure:** facing, commander detection, fine-grained mounted ratio (see adapter messages). |
| **`BackspaceConflictGuard`** | Coordinate Backspace with native order UI. | **No-op:** `ShouldSuppressNativeBackspace` always **`false`**. Engine does not expose a verified managed “block native Backspace” API (see `docs/research/base-game-order-scan.md` §8). |

## Why isolate native API calls?

- **Version drift:** public method names move (e.g. `MissionScreen` restore helpers on 1.3.x).
- **Ordering:** camera vs simulation tick order is easy to break if calls scatter across behaviors.
- **Audit:** one file per concern for reviewers and for “go/no-go” from Slice 0 research (`implementation-decision-slice0.md`).

## Commander camera (data only)

- **`CommanderCameraPose`** — `Position`, `Yaw`, `Pitch`, `Height` (fields).
- **`CommanderCameraController`** — `InitializeFromMission` / `InitializeFromAgent` / `GetPose` / `Reset`. **No movement integration** in Slice 3.

## `CommanderMissionView` integration

- Owns **`CommanderCameraController`**, **`CameraBridge`**, **`BackspaceConflictGuard`**.
- When commander mode is **enabled**, runs a **single** `CameraBridge.TryApply` probe (first eligible tick) and logs **at most one** debug line if the bridge is not applied — **no per-frame spam**.
- On cleanup, calls **`RestoreNativeCamera`** once (still **NotWired**, one debug line if not restored).

## What Slice 0 research still blocks

| Topic | Blocker |
| --- | --- |
| **Camera apply + restore** | Confirm `MissionScreen` + base types on **your** `TaleWorlds.MountAndBlade.View.dll`; optional `SetCameraLockState` semantics. |
| **`OrderPositionLock` / cavalry cadence** | ILSpy + runtime before writing charge / reform automation. |
| **Backspace suppression** | No safe public contract to steal Backspace from native order UI — guard stays passive. |

## See also

- [`docs/tests/manual-test-checklist-slice-3.md`](../tests/manual-test-checklist-slice-3.md)
- Research: `docs/research/base-game-camera-scan.md`, `base-game-order-scan.md`, `implementation-decision-slice0.md`
