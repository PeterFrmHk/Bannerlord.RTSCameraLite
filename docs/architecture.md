# Architecture — RTS Commander Doctrine

This document describes the **intended layered architecture** for `Bannerlord.RTSCameraLite`. It is a **portfolio and engineering map**; not every layer is fully implemented yet. See `docs/slice-roadmap.md` for delivery status.

---

## Layer stack (top → bottom)

```
[ Feedback ] ──► player-visible outcomes (messages, markers, future HUD)
        ▲
[ Native command execution ] ──► OrderController / Formation public primitives
        ▲
[ Doctrine ] ──► eligibility, morale/training hooks, cavalry sequences (planned)
        ▲
[ Commander ] ──► presence, anchor, rally targets (planned)
        ▲
[ Layout ] ──► row / rank / spacing planner (planned)
        ▲
[ Input ] ──► key read, ownership guard, routing
        ▲
[ Camera ] ──► pose + bridge application
        ▲
[ MissionView ] ──► MissionView-derived behavior, tick + camera override
        ▲
[ MissionLogic ] ──► MissionBehavior patterns where used (lifecycle, non-view logic)
```

### MissionView layer

- **Responsibility:** Engine hooks tied to the mission **screen** and **camera override** (`UpdateOverridenCamera`, screen init/finalize).
- **Why:** Bannerlord exposes the RTS camera extension point on **`MissionView`**, not on arbitrary `MissionBehavior` types alone.

### MissionLogic layer

- **Responsibility:** Mission-scoped logic that does not require view hooks (policies, caches, tick helpers).
- **Why:** Keeps non-rendering rules testable and avoids stuffing unrelated logic into the view type.

### Camera layer

- **Responsibility:** Pose integration, height/yaw/pitch rules, smoothing clamps.
- **Why:** Separates **feel** from **engine apply** (adapter).

### Input layer

- **Responsibility:** Snapshot building, RTS vs native ownership, conflict guards (including planned **Backspace** policy).
- **Why:** Centralizes “who owns this frame’s keys” to prevent double movement.

### Commander layer (planned)

- **Responsibility:** Commander presence model, anchor position behind formation face, rally targeting.
- **Why:** Encodes the **doctrine nucleus** separate from camera math.

### Doctrine layer (planned)

- **Responsibility:** Profiles (infantry vs cavalry), eligibility, morale/training gates, “no commander → no discipline” rules.
- **Why:** Gameplay policy should not be embedded inside camera or raw order calls.

### Layout layer (planned)

- **Responsibility:** Row/rank/spacing planner, absorption distances, cavalry width rules.
- **Why:** Geometry is an **output** of doctrine inputs.

### Native command execution layer

- **Responsibility:** Validated intents → **`OrderController`** / **`Formation`** calls, selection snapshot restore, failure reporting.
- **Why:** One choke point for **native** order compatibility.

### Feedback layer

- **Responsibility:** Throttled player messages, minimal markers, future tactical UI (optional).
- **Why:** Keeps player trust without coupling doctrine to Gauntlet.

---

## Adapter boundaries

### CameraBridge

- **Role:** Apply and restore camera frames against **`MissionScreen`** / engine camera surfaces using **build-scoped** reflection where needed.
- **Boundary:** Callers pass **`MissionView`**, **`Mission`**, pose, and `dt`; **no** scattered `GetMethod` across the codebase.
- **Outputs:** Structured result (applied vs failed) for throttled warnings.

### NativeOrderPrimitiveExecutor (concept) / `NativeOrderExecutor` (code today)

- **Role:** Issue **Charge**, **Hold**, **Move** (and future cavalry **sequences**) using **public** primitives (`OrderController.SetOrder`, `SetOrderWithPosition`, `WorldPosition`, etc.).
- **Boundary:** Upper layers pass **validated intents + context**; the executor owns **selection restoration** and **never throws** across the mod boundary.
- **Note:** The repository class is currently named **`NativeOrderExecutor`**; treat **`NativeOrderPrimitiveExecutor`** as the **architecture label** for “only engine primitives, no puppeteering.”

### FormationDataAdapter (planned)

- **Role:** Read formation rosters, classes, spacing hints, and future morale/training signals from **engine types** without leaking version-sensitive field access across layers.
- **Boundary:** Doctrine and layout layers depend on **narrow DTOs** (counts, class, flags), not raw `Formation` internals everywhere.

---

## Text diagram — call flow (move command, today’s spine)

```
Player key
   → Input layer (snapshot + guard)
     → Commander / Doctrine (planned gates)
       → Command router (validate)
         → NativeOrderExecutor (OrderController + WorldPosition)
           → Feedback layer (result + optional marker)
```

## Text diagram — camera flow

```
Mission tick / screen tick
  → Camera layer updates pose
    → MissionView.UpdateOverridenCamera
      → CameraBridge.TryApply(MissionView, Mission, pose, dt)
        → MissionScreen / Engine.Camera (adapter-specific)
```

---

## Principles

1. **Prefer public APIs** for orders and camera.
2. **Isolate reflection** inside adapters.
3. **No doctrine logic** inside camera apply paths.
4. **Single execution choke point** for native orders.
