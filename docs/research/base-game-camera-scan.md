# Base game camera & mission integration scan

**Scope:** Research only. No production mod code was changed for this document. **Slice 0 decisions:** see [`implementation-decision-slice0.md`](implementation-decision-slice0.md). Signatures and types below were taken from **local Steam-installed assemblies** via **PowerShell `[System.Reflection.Assembly]::LoadFrom` / reflection** (equivalent surface to ILSpy). If a member is marked **UNCERTAIN**, it was not re-verified on this pass or depends on runtime context.

**Related:** [`camera-hooks.md`](camera-hooks.md) (Slice 5 workflow), [`slice-hard-gates.md`](../slice-hard-gates.md).

---

## 1. Environment

| Field | Value |
| --- | --- |
| **Bannerlord install path** | `C:\Program Files (x86)\Steam\steamapps\common\Mount & Blade II Bannerlord` |
| **Game / module version (observed)** | Official **Native** module `SubModule.xml` reports **`v1.3.15`** (not the same as `TaleWorlds.*.dll` file version fields, which often read `1.0.0.0`). |
| **Primary engine bin (managed DLLs)** | `<install>\bin\Win64_Shipping_Client\` |
| **View assembly location (this install)** | **`TaleWorlds.MountAndBlade.View.dll` is not in the root `bin\Win64_Shipping_Client` listing**; it was found at `<install>\Modules\Native\bin\Win64_Shipping_Client\TaleWorlds.MountAndBlade.View.dll` (matches Native `SubModule` `DLLName`). |
| **Assemblies loaded for this scan** | From root bin: `TaleWorlds.MountAndBlade.dll`, `TaleWorlds.Engine.dll`, `TaleWorlds.Core.dll`, `TaleWorlds.Library.dll`, `TaleWorlds.InputSystem.dll`, `TaleWorlds.ScreenSystem.dll`, `TaleWorlds.MountAndBlade.ViewModelCollection.dll`, and other `TaleWorlds*.dll` in the same folder to satisfy `GetExportedTypes` dependencies. From Native module bin: `TaleWorlds.MountAndBlade.View.dll`. |
| **Tool** | PowerShell 5.x + .NET reflection (`LoadFrom`, `GetType`, `GetMethods`, `GetProperties`). |
| **Date** | **2026-04-17** (re-verified MissionScreen camera fields) |

---

## 2. Relevant classes found

| Assembly | Namespace | Class | Purpose | Access | Why relevant |
| --- | --- | --- | --- | --- | --- |
| `TaleWorlds.MountAndBlade.dll` | `TaleWorlds.MountAndBlade` | `Mission` | Runtime mission state, agents, teams, camera helpers. | **public** | `AddMissionBehavior`, `SetCameraFrame`, `GetCameraFrame`, mission tick pipeline. |
| `TaleWorlds.MountAndBlade.dll` | `TaleWorlds.MountAndBlade` | `MissionBehavior` | Base for mission-side logic; ticks with mission. | **public** | Extension point for custom logic; exposes `Mission`, `OnMissionTick`, lifecycle hooks. |
| `TaleWorlds.MountAndBlade.View.dll` | `TaleWorlds.MountAndBlade.View.MissionViews` | `MissionView` | View-layer mission behavior; camera override hook; screen lifecycle. | **public** | **RTS camera:** `UpdateOverridenCamera` (Bannerlord spelling **Overriden**), `Input`, `MissionScreen`, `Mission`. |
| `TaleWorlds.MountAndBlade.View.dll` | `TaleWorlds.MountAndBlade.View.Screens` | `MissionScreen` | Screen object for mission UI + cameras. | **public** | **`CombatCamera`**, **`UpdateFreeCamera`**, **`SetCameraLockState`**, etc. |
| `TaleWorlds.Engine.dll` | `TaleWorlds.Engine` | `Camera` | Engine camera entity. | **public** | **`Frame` property** (`get`/`set` with `MatrixFrame`) for applying view matrix. |
| `TaleWorlds.InputSystem.dll` | `TaleWorlds.InputSystem` | `IInputContext` | Abstract input read surface. | **public** interface | `IsKeyPressed`, `IsKeyDown`, mouse, controller. |
| `TaleWorlds.MountAndBlade.dll` | `TaleWorlds.InputSystem` (via `MissionView`) | *(usage)* | `MissionView` exposes `IInputContext` as `Input`. | **public** | Same-frame key polling inside mission views. |
| `TaleWorlds.MountAndBlade.ViewModelCollection.dll` | `TaleWorlds.MountAndBlade.ViewModelCollection` | `IMissionScreen` | VM-facing mission screen contract. | **public** | UI / Gauntlet side; **not** the same as `View.Screens.MissionScreen` concrete type. |

**Not found on exported-type scan (this pass):**

- **`AgentVisuals`**: no exported type with that exact name in `TaleWorlds.Core.dll` / `TaleWorlds.Engine.dll` quick export scan — **UNCERTAIN** (may be non-exported, renamed, or in another assembly).
- **`MissionMainAgentController`**, **`FreeCamera`**, **`Spectator`** as distinct public type names: **no hits** in `TaleWorlds.MountAndBlade.dll` type-name filter; free-camera behavior appears folded into **`MissionScreen.UpdateFreeCamera`** instead.

---

## 3. Relevant methods / properties (signatures from local DLLs)

### 3.1 `TaleWorlds.MountAndBlade.Mission` (`TaleWorlds.MountAndBlade.dll`)

| Member | Signature (approx.) | Access | Notes |
| --- | --- | --- | --- |
| `AddMissionBehavior` | `void AddMissionBehavior(MissionBehavior)` | **public** | Attach mod `MissionBehavior` / register view behaviors via module pipeline (mod `SubModule.xml` / game loader). |
| `RemoveMissionBehavior` | `void RemoveMissionBehavior(MissionBehavior)` | **public** | Cleanup. |
| `SetCameraFrame` | `void SetCameraFrame(ref MatrixFrame, float)` | **public** | Direct mission camera matrix update. |
| `SetCameraFrame` | `void SetCameraFrame(ref MatrixFrame, float, ref Vec3)` | **public** | Overload with extra `Vec3`. |
| `GetCameraFrame` | `MatrixFrame GetCameraFrame()` | **public** | Read current camera frame (candidate for restore snapshot). |
| `ResetFirstThirdPersonView` | `void ResetFirstThirdPersonView()` | **public** | Reset first/third person camera mode. |
| `CameraIsFirstPerson` | `bool get_CameraIsFirstPerson()` / `void set_CameraIsFirstPerson(bool)` | **public** | Camera mode flag. |
| `SetCustomCamera*` / getters | Various `void SetCustomCameraLocalOffset(Vec3)` etc. | **public** | Alternative “custom camera” offset API; interacts with native camera rig — treat as **higher coupling** until tested. |

**Candidate use:** **Option B** (apply/restore via `Mission` without touching `Engine.Camera` directly) or hybrid with `MissionView`.

---

### 3.2 `TaleWorlds.MountAndBlade.MissionBehavior` (`TaleWorlds.MountAndBlade.dll`)

| Member | Signature | Access | Notes |
| --- | --- | --- | --- |
| `Mission` | `Mission get_Mission()` | **public** | Access mission from behavior. |
| `OnBehaviorInitialize` | `void OnBehaviorInitialize()` | **public** | Behavior attach. |
| `OnRemoveBehavior` | `void OnRemoveBehavior()` | **public** | **Restore / cleanup hook.** |
| `OnMissionTick` | `void OnMissionTick(float)` | **public** | Per-frame mission tick. |
| `DebugInput` | `IInputContext get_DebugInput()` | **public** | Secondary input context on base behavior. |

### 3.2a Battle mission lifecycle (simulation vs. screen)

| Layer | Types / hooks | Role |
| --- | --- | --- |
| **Simulation** | `Mission` (`get_MissionEnded`, `get_MissionIsEnding`, `EndMission`, …), `MissionBehavior` (`OnMissionTick`, `OnEndMissionInternal`, …) | Gameplay state, agents, teams, `SetCameraFrame` family. |
| **Presentation** | `MissionView` (`OnMissionScreenTick`, `OnMissionScreenInitialize`, `OnMissionScreenFinalize`, `UpdateOverridenCamera`) | Camera override + view tick ordering. |
| **Screen** | `MissionScreen` (`Activate`, `Deactivate`, `HandleUserInput`, `IsOrderMenuOpen`) | UI + combat camera host. |

**UNCERTAIN:** exact tick order between `MissionScreen.HandleUserInput` and `MissionView.OnMissionScreenTick` relative to `MissionBehavior.OnMissionTick` — **requires** runtime logging on one mission.

---

### 3.3 `TaleWorlds.MountAndBlade.View.MissionViews.MissionView` (`TaleWorlds.MountAndBlade.View.dll`)

| Member | Signature | Access | Notes |
| --- | --- | --- | --- |
| `UpdateOverridenCamera` | `bool UpdateOverridenCamera(float dt)` | **public virtual** | **Primary Slice 5 hook:** return `true` when this view owns the camera for the frame. |
| `Mission` | `Mission get_Mission()` | **public** | Same as behavior mission reference. |
| `MissionScreen` | `MissionScreen get_MissionScreen()` | **public** | Type: **`TaleWorlds.MountAndBlade.View.Screens.MissionScreen`**. |
| `Input` | `IInputContext get_Input()` | **public** | Mission input polling. |
| `OnMissionScreenInitialize` | `void OnMissionScreenInitialize()` | **public** | Screen init. |
| `OnMissionScreenFinalize` | `void OnMissionScreenFinalize()` | **public** | Screen teardown. |
| `OnMissionScreenTick` | `void OnMissionScreenTick(float dt)` | **public** | Screen tick (distinct from `OnMissionTick` on `MissionBehavior`). |

**Note:** `OnMissionTick` exists on **`MissionBehavior`** base; `MissionView` adds screen/camera-oriented hooks above.

---

### 3.4 `TaleWorlds.MountAndBlade.View.Screens.MissionScreen` (`TaleWorlds.MountAndBlade.View.dll`)

| Member | Signature | Access | Candidate use |
| --- | --- | --- | --- |
| `CombatCamera` | `Camera get_CombatCamera()` | **public** | Resolve `TaleWorlds.Engine.Camera`; set `Frame` for RTS view. |
| `CustomCamera` | `Camera get_CustomCamera()` / `void set_CustomCamera(Camera)` | **public** | Alternate camera slot — **UNCERTAIN** semantics without in-game test. |
| `UpdateFreeCamera` | `void UpdateFreeCamera(MatrixFrame frame)` | **public** | **Explicit free-camera update** path on the mission screen. |
| `SetCameraLockState` | `void SetCameraLockState(bool isLocked)` | **public** | May reduce native camera fighting RTS — **UNCERTAIN** side effects. |
| `SetExtraCameraParameters` | `void SetExtraCameraParameters(bool newForceCanZoom, float newCameraRayCastStartingPointOffset)` | **public** | Tuning — **UNCERTAIN** value ranges. |
| `LockCameraMovement` | `bool get_LockCameraMovement()` | **public** | Query lock state. |
| `Mission` | `Mission get_Mission()` | **public** | Link back to simulation. |
| `CameraBearing` | `float get_CameraBearing()` / `void set_CameraBearing(float)` | **public** | Native bearing drive; optional mirror for RTS pose sync — **UNCERTAIN** interaction when overriding `CombatCamera.Frame`. |
| `CameraElevation` | `float get_CameraElevation()` / `void set_CameraElevation(float)` | **public** | Same as above. |
| `MaxCameraZoom` | `float get_MaxCameraZoom()` | **public** | Zoom ceiling helper (`Mission.GetMainAgentMaxCameraZoom()` exists separately on simulation). |
| `CameraResultDistanceToTarget` | `float get_CameraResultDistanceToTarget()` | **public** | Distance readout — **UNCERTAIN** use for RTS. |
| `CameraViewAngle` | `float get_CameraViewAngle()` | **public** | FOV-related — **UNCERTAIN**. |
| `IsMissionTickable` | `bool get_IsMissionTickable()` | **public** | Gate work when simulation paused — **UNCERTAIN** all modes. |
| `IsOrderMenuOpen` | `bool get_IsOrderMenuOpen()` | **public** | **Input / UI coupling:** use with RTS toggle design — see [`implementation-decision-slice0.md`](implementation-decision-slice0.md). |
| `InputManager` | `IInputContext get_InputManager()` | **public** | Alternate input surface vs. `MissionView.Input` — **UNCERTAIN** which matches battle keys on this build. |
| `HandleUserInput` | `void HandleUserInput(float dt)` | **public** | Native per-frame input routing — **Backspace / order menu conflict** requires ILSpy body read (**UNCERTAIN** from signatures alone). |

**Restore / naming gap (important):** A quick export scan of `TaleWorlds.MountAndBlade.View.dll` on **this 1.3.15 install** did **not** surface public methods named **`ActivateMainAgentCamera`**, **`ResetCameraMode`**, **`SwitchToDefaultCameraMode`**, or **`ActivateMainAgentSpectatorCamera`** (names the mod’s `CameraBridge` tries via reflection). Those names may be **removed, renamed, moved, or non-public** on 1.3.x — treat as **UNCERTAIN / build-specific**. Re-scan with ILSpy on **your** `MissionScreen` inheritance chain before relying on them.

---

### 3.5 `TaleWorlds.Engine.Camera` (`TaleWorlds.Engine.dll`)

| Member | Signature | Access | Candidate use |
| --- | --- | --- | --- |
| `Frame` | `MatrixFrame get_Frame()` / `void set_Frame(MatrixFrame)` | **public** | Apply `MatrixFrame` from RTS pose when holding a `Camera` reference (e.g. `CombatCamera`). |

---

### 3.6 `TaleWorlds.InputSystem.IInputContext` (`TaleWorlds.InputSystem.dll`)

| Member | Signature | Access | Candidate use |
| --- | --- | --- | --- |
| `IsKeyDown` | `bool IsKeyDown(InputKey)` | **public** | Held keys. |
| `IsKeyPressed` | `bool IsKeyPressed(InputKey)` | **public** | Edge-pressed keys. |
| `IsKeyReleased` | `bool IsKeyReleased(InputKey)` | **public** | Edge release. |
| *(also)* | `IsGameKeyPressed(int)`, hotkey helpers, mouse deltas | **public** | Alternate binding paths. |

---

## 4. Candidate camera application paths (ranked)

### Option A — `MissionView.UpdateOverridenCamera` + `MissionScreen.CombatCamera.Frame`

| Aspect | Assessment |
| --- | --- |
| **Required API calls** | `MissionView.UpdateOverridenCamera(dt)` returns `true`; inside, read `MissionScreen.CombatCamera`, assign `camera.Frame = matrixFrame` (or call `SetCameraFrame` on `Camera` if preferred — **`set_Frame` observed public**). |
| **Public?** | **Yes** (types and members listed above). |
| **Harmony needed?** | **No** for the core hook + `Camera.Frame`. |
| **Risk** | **Medium:** native code may also write the same camera same frame; must release override (`return false`) when RTS off. |
| **Restore strategy** | Stop returning `true` from `UpdateOverridenCamera`; optionally call `Mission.ResetFirstThirdPersonView()` and/or re-apply `Mission.GetCameraFrame()` snapshot — **ordering UNCERTAIN** without playtest. |
| **Accept / reject** | **Accept** as primary mod architecture (matches existing `RTSCameraMissionBehavior : MissionView`). |

---

### Option B — `Mission.SetCameraFrame` / `Mission.GetCameraFrame` (simulation-level)

| Aspect | Assessment |
| --- | --- |
| **Required API calls** | `mission.SetCameraFrame(ref frame, float)`; `mission.GetCameraFrame()` for snapshot. |
| **Public?** | **Yes** (declared on `Mission`). |
| **Harmony needed?** | **No**. |
| **Risk** | **Medium–high:** bypasses `MissionView` contract; may fight `MissionScreen` updates or order-of-operations vs. view tick — **needs in-game verification**. |
| **Restore strategy** | Restore saved `MatrixFrame` via `SetCameraFrame`, or `ResetFirstThirdPersonView()`. |
| **Accept / reject** | **Accept as fallback / hybrid** if `CombatCamera` path fails on a build; **reject as sole path** until verified it does not desync from rendering camera. |

---

### Option C — `MissionScreen.UpdateFreeCamera(MatrixFrame)`

| Aspect | Assessment |
| --- | --- |
| **Required API calls** | Obtain `MissionScreen` from `MissionView.MissionScreen`, call `UpdateFreeCamera(frame)`. |
| **Public?** | **Yes**. |
| **Harmony needed?** | **No**. |
| **Risk** | **UNCERTAIN:** semantics of “free camera” vs. combat camera on 1.3.15 (interaction with lock state, deployment, death cam). |
| **Restore strategy** | Stop calling; pair with `SetCameraLockState` / native resets — **UNCERTAIN**. |
| **Accept / reject** | **Hold as experiment** behind feature flag or build-gated probe; **do not rely on exclusively** without tests. |

---

## 5. Input handling findings

- **Key read APIs:** `MissionView.Input` → `IInputContext.IsKeyPressed(InputKey)`, `IsKeyDown`, `IsKeyReleased` (all **public** on the interface).
- **Contexts:** `MissionView.Input` and base `MissionBehavior.DebugInput` — **two contexts** exist; which matches gameplay intent is **UNCERTAIN** without comparing behavior in deployment vs. battle.
- **Native input suppression:** Not conclusively determined from this DLL-only scan. **UNCERTAIN** whether engine ignores movement when a `MissionView` “owns” input; mod currently uses a separate input guard (see repo `NativeInputGuard`) — **architectural**, not proven from these signatures alone.
- **Conflict risk:** High overlap with **agent movement** (WASD), **order keys**, and **banner keys** — binding tables must be configurable (mod already uses JSON keys).

---

## 6. Restore behavior findings

- **Native control:** `MissionScreen` owns **`CombatCamera`** (`TaleWorlds.Engine.Camera`) and exposes **`UpdateFreeCamera`**, bearing/elevation properties, and **`SetCameraLockState`**. `Mission` exposes **`SetCameraFrame` / `GetCameraFrame`** and **`ResetFirstThirdPersonView`**.
- **Stop applying RTS camera:** Return **`false`** from `MissionView.UpdateOverridenCamera` when RTS mode is off (releases override for that view).
- **Avoid camera lock / fight:** **`SetCameraLockState(bool)`** exists — **UNCERTAIN** when toggling is safe (could affect UI/cutscenes).
- **Mission end cleanup:** `MissionBehavior.OnRemoveBehavior` / `MissionView` end paths — mod should **`TryRestore`** camera and clear state (matches existing `CameraBridge.TryRestore` intent).

**Gap:** Parameterless **`ActivateMainAgentCamera`**-style restore methods were **not observed** on `MissionScreen` public surface for this 1.3.15 View DLL scan — **re-verify with ILSpy** on the full type hierarchy and any **`Gauntlet`** / **`ScreenSystem`** base classes.

---

## 7. Recommended implementation path (Slice 5)

**Recommended (matches current mod direction):**

1. **Attachment:** Keep a **`MissionView`** subclass in the mod module (`RTSCameraMissionBehavior` pattern) so **`UpdateOverridenCamera(float dt)`** is available without Harmony.
2. **Apply path (primary):** From `MissionView.MissionScreen` → **`CombatCamera`** → **`Camera.set_Frame(MatrixFrame)`** *or* call **`MissionScreen.UpdateFreeCamera(MatrixFrame)`** only after proving equivalence in-game.
3. **Apply path (fallback):** **`Mission.SetCameraFrame(ref MatrixFrame, float)`** if the `CombatCamera` reference is null or `Frame` assignment is ineffective on a given build.
4. **Harmony:** **Not required** for the public hooks above. Consider **smallest Harmony patch** only if a future build removes `UpdateOverridenCamera` or makes `CombatCamera` inaccessible — **not indicated on this 1.3.15 scan**.
5. **`CameraBridge` adapter boundary:** Keep all **reflection** and **method-name probing** inside `CameraBridge` (or successor), fed only **`MissionView`**, **`Mission`**, **`RTSCameraPose`**, and **`dt`**. Update restore name list against **local** `MissionScreen` + base types after ILSpy pass on **1.3.15**.

**Exact signatures to pin in code comments (from this scan):**

- `bool MissionView.UpdateOverridenCamera(float dt)` — *note spelling* **`Overriden`**.
- `void MissionScreen.UpdateFreeCamera(MatrixFrame frame)`.
- `Camera MissionScreen.CombatCamera { get; }`.
- `void Camera.set_Frame(MatrixFrame value)` on `TaleWorlds.Engine.Camera`.
- `void Mission.SetCameraFrame(ref MatrixFrame, float)` / `MatrixFrame Mission.GetCameraFrame()`.
- `void Mission.ResetFirstThirdPersonView()`.

---

## 8. Follow-up for owners

1. Re-run ILSpy on **`TaleWorlds.MountAndBlade.View.dll`** (`Modules\Native\bin\Win64_Shipping_Client`) for **`MissionScreen`** and **all base types** to rediscover any **restore** entry points for **1.3.15**.  
2. In-game: one mission logging whether **`CombatCamera`** is non-null when RTS toggles on.  
3. Align `docs/version-lock.md` with **launcher build string** (not only Native `v1.3.15` tag).
