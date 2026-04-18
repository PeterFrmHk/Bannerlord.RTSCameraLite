# Slice 3 Audit: RTS camera controller skeleton + `CameraBridge`

## Purpose

Introduce **`RTSCameraPose`**, **`RTSCameraController`**, and **`CameraBridge`** so RTS state (Slice 2) can drive a **pose model** without WASD movement, without Harmony, and without scattering native camera calls outside the bridge.

## Function spec

- **Pose:** `Vec3` position, yaw, pitch, height.
- **Controller:** `InitializeFromAgent`, `GetPose`, `Reset`; defaults pitch **60f**, height **18f**; no movement logic.
- **Bridge:** `TryApply(Mission, RTSCameraPose)` → `CameraBridgeResult`; all future Bannerlord camera APIs **only** in `CameraBridge`; until wired, returns **not applied** with message **`Camera bridge not wired`**.
- **Mission behavior:** When RTS is **enabled**, ensure controller init from **`Mission.MainAgent`**, call **`CameraBridge.TryApply`**, log bridge message **once per enable** (not every tick).

## PRISM

- **A:** Move the camera immediately with full RTS controls.
- **¬A:** Camera APIs differ by build; controller and bridge must exist before motion.
- **A\*:** Slice 3 adds pose + bridge seam + safe no-op; movement comes later.

## Tests

- [ ] `dotnet build` passes
- [ ] Custom battle loads
- [ ] F10 enables RTS state
- [ ] Controller initializes pose from player position when `MainAgent` exists
- [ ] `CameraBridge.TryApply` runs safely while enabled
- [ ] If bridge is not wired, **no crash**
- [ ] No Harmony, no WASD movement, no settings UI

## Audit

### Implemented

- `src/Camera/RTSCameraPose.cs`
- `src/Camera/RTSCameraController.cs`
- `src/Camera/CameraBridge.cs`
- `src/Camera/CameraBridgeResult.cs`
- `src/Mission/RTSCameraMissionBehavior.cs` integration

### Not implemented

- Real camera matrix / `MissionScreen` wiring inside `CameraBridge`
- WASD / mouse orbit / zoom input
- Harmony, MCM, UI

### Risks

- `MainAgent` timing: init may wait until agent exists; bridge stays no-op until Slice 4+.
- Yaw from `LookDirection` is a placeholder heuristic; refine per engine conventions later.

### Next slice readiness

- [ ] In-game: one log line per F10-enable with `Camera bridge not wired` (expected until wired).
- [ ] No regressions on F10 toggle or battle exit.
