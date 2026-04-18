# Base game order & formation scan (research only)

**Scope:** Research only. No production mod code (`src/`) was changed for this document. All **signatures** below were read from **local Steam-installed** managed DLLs via **PowerShell reflection** (`Assembly.LoadFrom`, `GetMethod`, `GetProperty`, `GetEnumNames`). Where a fact could not be confirmed from those assemblies alone, it is marked **UNCERTAIN**.

**Related:** [`native-order-hooks.md`](native-order-hooks.md) (narrow order-API note), [`slice-hard-gates.md`](../slice-hard-gates.md) (Slice 12 research gate).

---

## 1. Environment

| Field | Value |
| --- | --- |
| **Bannerlord install path** | `C:\Program Files (x86)\Steam\steamapps\common\Mount & Blade II Bannerlord` |
| **Game / module version (observed)** | Official **Native** module `Modules\Native\SubModule.xml` reports **`v1.3.15`**. TaleWorlds managed assemblies still report **FileVersion `1.0.0.0`** in many cases — **do not** use file version as gameplay build ID. |
| **Assemblies inspected (paths)** | **Primary:** `<install>\bin\Win64_Shipping_Client\` — loaded all `TaleWorlds*.dll` there to satisfy dependencies, then inspected: `TaleWorlds.MountAndBlade.dll`, `TaleWorlds.Core.dll`, `TaleWorlds.Library.dll`, `TaleWorlds.Engine.dll`. **`TaleWorlds.MountAndBlade.View.dll`** was **not required** for the order primitives scanned here (orders are on simulation types in `MountAndBlade.dll`). |
| **Tool** | PowerShell 5.x + .NET reflection. |
| **Date** | **2026-04-18** |

---

## 2. Relevant classes found

| Assembly | Namespace | Class | Purpose | Access | Why relevant |
| --- | --- | --- | --- | --- | --- |
| `TaleWorlds.MountAndBlade.dll` | `TaleWorlds.MountAndBlade` | `Mission` | Active mission simulation. | **public** | Provides `MainAgent`, `Scene`, mission-ended state; anchor for team/order context. |
| `TaleWorlds.MountAndBlade.dll` | `TaleWorlds.MountAndBlade` | `Team` | Side’s formations + order controllers. | **public** | **`GetFormation(FormationClass)`**, **`PlayerOrderController`**, **`GetOrderControllerOf(Agent)`**, order events. |
| `TaleWorlds.MountAndBlade.dll` | `TaleWorlds.MountAndBlade` | `Formation` | A tactical formation instance. | **public** | **`SetMovementOrder`**, counts, orders, AI flags. |
| `TaleWorlds.MountAndBlade.dll` | `TaleWorlds.MountAndBlade` | `OrderController` | Issues UI-style orders to **selected** formations. | **public** | **`SetOrder`**, **`SetOrderWithPosition`**, selection APIs. |
| `TaleWorlds.MountAndBlade.dll` | `TaleWorlds.MountAndBlade` | `MovementOrder` | Struct + factories for movement orders. | **public** | **`MovementOrderMove(WorldPosition)`**, **`MovementOrderChargeToTarget(Formation)`**, etc. |
| `TaleWorlds.MountAndBlade.dll` | `TaleWorlds.MountAndBlade` | `OrderType` | Enum of discrete order kinds. | **public** | **`Charge`**, **`StandYourGround`**, **`Move`**, retreat/advance, arrangements, etc. |
| `TaleWorlds.MountAndBlade.dll` | `TaleWorlds.MountAndBlade` | `FacingOrder`, `ArrangementOrder`, `FormOrder`, `RidingOrder`, `FiringOrder` | Non-movement order facets. | **public** | `Formation.SetFacingOrder`, `SetArrangementOrder`, `SetFormOrder`, … |
| `TaleWorlds.MountAndBlade.dll` | `TaleWorlds.MountAndBlade` | `OnOrderIssuedDelegate` | Multicast delegate for order-issued callbacks. | **public** | Signature for **`Team.add_OnOrderIssued`**. |
| `TaleWorlds.Core.dll` | `TaleWorlds.Core` | `FormationClass` | Slot / class key for `Team.GetFormation`. | **public** enum | Stable formation indexing. |
| `TaleWorlds.Engine.dll` | `TaleWorlds.Engine` | `WorldPosition` | Scene-bound position for orders / placement. | **public** | **`WorldPosition(Scene, Vec3)`** ctor (used with `OrderType.Move`). |

**Not found (this scan):**

- **Type name `MissionOrder`** as a public class in `TaleWorlds.MountAndBlade.dll` — **not located** in a quick type-name sweep (**UNCERTAIN** if renamed, internal-only, or obsolete on this build).
- **Type name `OrderSet`** — same (**not located** on quick sweep).

---

## 3. Formation model

### Representation

- A **`Team`** owns multiple **`Formation`** instances keyed by **`TaleWorlds.Core.FormationClass`** via **`Team.GetFormation(FormationClass)`** (public instance method observed on `Team`).
- The team also exposes lists such as **`Team.FormationsIncludingEmpty`** / **`FormationsIncludingSpecialAndEmpty`** (`MBList<Formation>` getters observed on `Team`) for broader iteration (**exact semantics UNCERTAIN** without in-game probing).

### Friendly vs enemy

- **Friendly team** for the human player is typically reached from **`Mission.MainAgent`** → **`Agent.Team`** (**public**; observed pattern in engine APIs).
- **Enemy** formations are the other **`Team`** objects on the mission (**UNCERTAIN** which helper enumerates all teams fastest — use mission APIs appropriate to your build after ILSpy on `Mission`).

### Validity / empty / dead

- **`Formation.CountOfUnits`** (**public**, `int`): primary **empty** signal when `0`.
- **`Formation.HasPlayerControlledTroop`**, **`IsPlayerTroopInFormation`**, **`IsAIControlled`**, etc. (**public** properties observed): useful for gating player-issued orders.
- **Dead / routing:** **UNCERTAIN** from DLL scan alone; validate with combat tests (morale, routing flags may live on `Agent` / `Team`).

### **Important API note (this 1.3.15 Steam `Formation` surface)**

On the inspected **`TaleWorlds.MountAndBlade.dll`**, reflection **`GetProperty("Team")` on `Formation` returned `null`** — i.e. **no `Team` property** was discovered on **`Formation`** with default binding. A **`PlayerOwner`** property (**type `Agent`**) **was** observed.

- **Implication:** Code that assumes **`formation.Team`** may be tied to a **different reference assembly** than this Steam runtime, or an **older API**. **Re-verify with ILSpy** on the DLL your mod actually references.
- **Candidate replacement (UNCERTAIN semantics):** derive affiliation via **`formation.PlayerOwner`** → **`Agent.Team`**, or by proving the formation reference came from **`MainAgent.Team.GetFormation(...)`** when building the friendly list.

---

## 4. Order model

### Order classes / enums (observed)

| Kind | Type | Notes |
| --- | --- | --- |
| Discrete order kinds | **`OrderType`** enum | Includes **`Charge`**, **`StandYourGround`**, **`Move`**, **`Retreat`**, **`Advance`**, **`FaceDirection`**, arrangement/cohesion/mount/AI control values, **`HoldFire`**, etc. (enum names enumerated via reflection). |
| Movement payload | **`MovementOrder`** struct | Public **static** factories observed (see below). |
| Facets | **`FacingOrder`**, **`ArrangementOrder`**, **`FormOrder`**, … | Applied through **`Formation.SetFacingOrder`**, **`SetArrangementOrder`**, **`SetFormOrder`**, … |

### `MovementOrder` public static factories (observed)

| Method | Signature | Notes |
| --- | --- | --- |
| `MovementOrderMove` | `static MovementOrder MovementOrderMove(WorldPosition)` | Positional move. |
| `MovementOrderChargeToTarget` | `static MovementOrder MovementOrderChargeToTarget(Formation)` | **Charge toward a target `Formation`** (typically enemy). |
| `MovementOrderFollow` | `static MovementOrder MovementOrderFollow(Agent)` | Follow agent. |
| `MovementOrderFollowEntity` | `static MovementOrder MovementOrderFollowEntity(GameEntity)` | Follow entity. |
| `MovementOrderAttackEntity` | `static MovementOrder MovementOrderAttackEntity(GameEntity, bool)` | Attack entity. |

**Hold / “stop”:** There is **no** `MovementOrderHold` static factory in the **public static** list above. **Holding ground** maps naturally to **`OrderType.StandYourGround`** on **`OrderController.SetOrder`** (see native issuance below). **UNCERTAIN** whether a pure `MovementOrder` “stop” exists only via internal construction.

### Charge vs move vs hold (practical mapping)

| Intent | Observed public mapping |
| --- | --- |
| **Move to point** | **`OrderController.SetOrderWithPosition(OrderType.Move, WorldPosition)`** **or** **`Formation.SetMovementOrder(MovementOrder.MovementOrderMove(WorldPosition))`**. |
| **Charge (generic)** | **`OrderController.SetOrder(OrderType.Charge)`** on selected formations. |
| **Charge to specific enemy formation** | **`MovementOrder.MovementOrderChargeToTarget(Formation)`** + **`Formation.SetMovementOrder`** — requires a **target `Formation`**. |
| **Hold ground** | **`OrderController.SetOrder(OrderType.StandYourGround)`** — **no extra position**. |

### How the native game issues player orders (high level, from APIs)

1. Resolve an **`OrderController`** for the player context: **`Team.GetOrderControllerOf(Agent)`** or fallback **`Team.PlayerOrderController`** (both **public** getters/methods on `Team`).
2. Ensure target formations are **selected**: **`OrderController.SelectedFormations`** is an **`MBReadOnlyList<Formation>`**; mutate with **`ClearSelectedFormations`**, **`SelectFormation`**, **`DeselectFormation`**, **`SelectAllFormations`** (**all public**).
3. Issue: **`SetOrder(OrderType)`** or **`SetOrderWithPosition(OrderType, WorldPosition)`**, plus other overloads with agent / two positions / orderable object (**all public** on `OrderController`).
4. Alternatively, bypass selection UI by calling **`Formation.SetMovementOrder(MovementOrder)`** directly (**public**).

**Event surface:** **`Team.add_OnOrderIssued(OnOrderIssuedDelegate)`** / remove — delegate **`Invoke`** signature observed as:

`void Invoke(OrderType orderType, MBReadOnlyList<Formation> formations, OrderController orderController, object[] array)`

(**public** multicast pattern.)

---

## 5. Candidate execution paths

### Option A — **`OrderController` public methods** (selection + `SetOrder` / `SetOrderWithPosition`)

| Item | Detail |
| --- | --- |
| **Classes / methods** | `Team.GetOrderControllerOf(Agent)` or `PlayerOrderController`; `OrderController.ClearSelectedFormations`, `SelectFormation`, `SetOrder(OrderType)`, `SetOrderWithPosition(OrderType, WorldPosition)`. |
| **Access** | **Public** (observed). |
| **Risk** | **Medium:** mutating **`SelectedFormations`** can desync native order UI until restored; must restore selection if mimicking partial UI selection. |
| **Compatibility** | **High** if signatures remain stable (they are first-class engine APIs). |
| **Harmony** | **Not required** for this path on the inspected build. |
| **Verdict** | **Accept** as **primary** RTS issuance path for **Charge / Hold / Move** when combined with validation + friendly formation checks. |

---

### Option B — **`Formation.SetMovementOrder(MovementOrder)`** (direct)

| Item | Detail |
| --- | --- |
| **Classes / methods** | `Formation.SetMovementOrder(MovementOrder)`; factories `MovementOrderMove`, `MovementOrderChargeToTarget`, … |
| **Access** | **Public**. |
| **Risk** | **Medium:** bypasses `OrderController` selection; native UI may not reflect the order; **Charge** without enemy target is awkward (`MovementOrderChargeToTarget` requires a `Formation`). |
| **Harmony** | **Not required**. |
| **Verdict** | **Accept** for **Move** when you already hold a `Formation` reference; **partial** for **Charge**; pair **Hold** with **`OrderType`** path instead of guessing a `MovementOrder` “stop”. |

---

### Option C — **Harmony / patch internal order pipeline**

| Item | Detail |
| --- | --- |
| **When** | Only if a future build **hides** `OrderController` / `SetMovementOrder` or adds mandatory middleware that cannot be satisfied publicly. |
| **Risk** | **High** (patch drift, multiplayer, anti-cheat perception). |
| **Verdict** | **Reject** unless **Option A/B** is proven impossible on a pinned game version. |

---

### Option D — **Simulate native input / order UI**

| Item | Detail |
| --- | --- |
| **Idea** | Synthesize clicks / hotkeys into the order UI layer. |
| **Risk** | **Very high** (focus, Gauntlet layering, localization, rebinding). |
| **Verdict** | **Reject** for RTS mod core; keep as **UNCERTAIN** last resort only. |

---

## 6. Minimal command execution plan (per command)

Parameters assume **`Mission.MainAgent`** non-null, **`Mission.Scene`** non-null for `WorldPosition`, and a resolved friendly **`Formation`** with **`CountOfUnits > 0`**.

### Charge

| Item | Detail |
| --- | --- |
| **Path A (recommended)** | `OrderController.SetOrder(OrderType.Charge)` after selecting the player formation(s). |
| **Path B** | `Formation.SetMovementOrder(MovementOrder.MovementOrderChargeToTarget(enemyFormation))` — requires **enemy** `Formation`; not “generic charge”. |
| **Parameters** | `OrderType` enum value **`Charge`**. |
| **Failures** | No `OrderController`; formation not **`IsFormationSelectable`**; mission ended; **0** units. |
| **Cleanup** | Restore **`SelectedFormations`** snapshot if temporarily cleared/changed. |

### Hold position

| Item | Detail |
| --- | --- |
| **Path** | **`OrderController.SetOrder(OrderType.StandYourGround)`** (maps to “hold ground” in `OrderType` list). |
| **Parameters** | None beyond selection. |
| **Failures** | Same as Charge regarding controller/selectability/units. |
| **Cleanup** | Selection restore as above. |

### Move to position

| Item | Detail |
| --- | --- |
| **Path A (recommended)** | **`OrderController.SetOrderWithPosition(OrderType.Move, new WorldPosition(Mission.Scene, Vec3))`**. |
| **Path B** | **`Formation.SetMovementOrder(MovementOrder.MovementOrderMove(new WorldPosition(Mission.Scene, Vec3)))`**. |
| **Parameters** | `WorldPosition` requires a valid **`Scene`** + finite **`Vec3`**. |
| **Failures** | Null scene; non-finite vector; `WorldPosition` construction throws (**UNCERTAIN** — wrap at callsite in mod code, not in this doc). |
| **Cleanup** | Selection restore; none for direct `Formation` path beyond leaving simulation consistent. |

---

## 7. Recommended implementation boundary (`NativeOrderExecutor`)

**Goal:** keep version-sensitive assumptions in **one** place.

| Rule | Rationale |
| --- | --- |
| **`NativeOrderExecutor` (or a single `OrderIssuanceAdapter` type)** is the **only** module that should call `OrderController` / `SetMovementOrder` / `WorldPosition` for RTS commands. | Prevents scatter of `Team`/`OrderController` rules. |
| **Callers pass** `Mission`, `Agent` (main), `Formation`, and **`WorldPosition?` / `Vec3?`** already validated. | Keeps executor thin and testable. |
| **Selection policy** (snapshot/restore `SelectedFormations`) lives **inside** the executor (or a nested helper), not in `MissionView`. | Matches UI coupling containment. |
| **Do not** read private fields on `OrderController` / `Formation` for this slice. | Prefer **public** APIs only. |
| **If `Formation.Team` is absent** on your runtime DLL, **stop** and re-run ILSpy; add a tiny **`FormationAffiliation`** helper rather than sprinkling reflection. | Addresses the **1.3.15 `Team` property** discrepancy observed here. |

---

## 8. Owner follow-ups

1. ILSpy **`TaleWorlds.MountAndBlade.Formation`** on **the exact DLL** your mod compiles against — confirm whether **`Team`** exists or was replaced by **`PlayerOwner`** / other means on **v1.3.15**.  
2. In-game verify **`IsFormationSelectable`** vs **`IsFormationListening`** for player-issued RTS orders during deployment vs battle.  
3. Keep this document next to **`docs/research/native-order-hooks.md`**; the latter stays the **Slice 12 hook cheat sheet**, this file is the **broader formation/order model**.
