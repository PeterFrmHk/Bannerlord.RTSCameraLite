# Slice 3 Audit: RTS camera controller skeleton + `CameraBridge`

## Purpose

Pose model (`RTSCameraPose`), controller (`RTSCameraController`), and **`CameraBridge`** as the **only** place for version-sensitive Bannerlord camera calls—without WASD, Harmony, or settings UI.

## Function spec (minimal shape)

- **`RTSCameraPose`:** internal sealed class; mutable `Position`, `Yaw`, `Pitch`, `Height`.
- **`RTSCameraController`:** `InitializeFromAgent(Agent)` (null-safe base position), `GetPose()`, `Reset()`; default height **18f**, pitch **60f**; position lifted by height on Z; **Yaw** **0f** for now.
- **`CameraBridge`:** internal sealed class; instance **`TryApply(TaleWorlds.MountAndBlade.Mission, RTSCameraPose)`**; null checks → `"Mission or pose missing"`; otherwise **`NotWired()`** until assemblies are inspected and wired.
- **`CameraBridgeResult`:** internal readonly struct; **`NotWired()`**, **`Success()`** for future use.
- **`RTSCameraMissionBehavior`:** holds **`CameraBridge`** instance; while RTS enabled, updates pose from **`MainAgent`**, calls **`TryApply`**, logs result **once per enable**.

## PRISM

- **A:** Directly manipulate camera from mission behavior.
- **¬A:** Bannerlord camera APIs are version-sensitive and must be isolated.
- **A\*:** Add **`CameraBridge`** as the only camera API contact point.

## Tests

- [ ] Build passes
- [ ] Custom battle loads
- [ ] F10 enables RTS state
- [ ] Camera controller initializes
- [ ] `CameraBridge.TryApply` is reached
- [ ] No crash while bridge is no-op
- [ ] No camera movement yet

## Audit

### Implemented

- `src/Camera/RTSCameraPose.cs`, `RTSCameraController.cs`, `CameraBridge.cs`, `CameraBridgeResult.cs`
- Mission behavior uses **`CameraBridge`** instance only (no static camera calls outside bridge).

### Not implemented

- Real camera / `MissionScreen` wiring inside **`CameraBridge.TryApply`**
- WASD / mouse movement, Harmony, MCM

### Risks

- **`Mission` identifier** in mission behavior: local variable typed **`TaleWorlds.MountAndBlade.Mission`** avoids **`Bannerlord.RTSCameraLite.Mission`** namespace clash at assignment from **`Mission`** property.

### Next slice readiness

- [ ] In-game: one log per enable with `Camera bridge not wired` until wired.
- [ ] Inspect local assemblies; implement **`TryApply`** body; return **`Success()`** when applied.
