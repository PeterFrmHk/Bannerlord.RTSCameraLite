# Native order hooks — research notes (Slice 12)

Completing this research for **your** Bannerlord build satisfies the **Slice 12** prerequisite in [`../slice-hard-gates.md`](../slice-hard-gates.md).

**Broader formation/order model & execution options:** see [`base-game-order-scan.md`](base-game-order-scan.md) (local Steam `TaleWorlds.MountAndBlade.dll` reflection, 2026-04-18).

## How this was inspected

Local game binaries were inspected with **PowerShell `[System.Reflection.Assembly]::LoadFrom`** and member enumeration (same surface area ILSpy would show for public API discovery). Paths used on the research machine:

| Assembly | Path (Steam layout) |
|----------|---------------------|
| `TaleWorlds.MountAndBlade.dll` | `Mount & Blade II Bannerlord\bin\Win64_Shipping_Client\TaleWorlds.MountAndBlade.dll` |
| `TaleWorlds.Engine.dll` | `...\bin\Win64_Shipping_Client\TaleWorlds.Engine.dll` |

Repo compile references: `Bannerlord.ReferenceAssemblies` **1.2.12.66233** (NuGet) when `BannerlordGameFolder` / `BANNERLORD_INSTALL` is unset.

---

## `OrderType` (enum, public)

**Assembly:** `TaleWorlds.MountAndBlade`  
**Type:** `TaleWorlds.MountAndBlade.OrderType`  

Relevant members for this slice (names only):

- `Charge`
- `StandYourGround`
- `Move`

(Full enum includes arrangement, cohesion, mount, AI control, etc.)

---

## `Team` — order controllers

**Assembly:** `TaleWorlds.MountAndBlade`  
**Type:** `TaleWorlds.MountAndBlade.Team`  

| Member | Signature | Access | Notes |
|--------|-----------|--------|-------|
| `PlayerOrderController` | `OrderController get_PlayerOrderController()` | public | Fallback when captain-specific controller is absent. |
| `GetOrderControllerOf` | `OrderController GetOrderControllerOf(Agent)` | public | Preferred when the player’s `MainAgent` may not use the team-wide player controller. |
| `MasterOrderController` | `OrderController get_MasterOrderController()` | public | Not used in Slice 12. |

---

## `OrderController` — issuance surface

**Assembly:** `TaleWorlds.MountAndBlade`  
**Type:** `TaleWorlds.MountAndBlade.OrderController`  

| Member | Signature | Access | Harmony required? |
|--------|-----------|--------|-------------------|
| `SelectedFormations` | `MBReadOnlyList<Formation> get_SelectedFormations()` | public | No |
| `ClearSelectedFormations` | `void ClearSelectedFormations()` | public | No |
| `SelectFormation` | `void SelectFormation(Formation)` | public | No |
| `IsFormationSelectable` | `bool IsFormationSelectable(Formation)` | public | No |
| `SetOrder` | `void SetOrder(OrderType)` | public | No |
| `SetOrderWithPosition` | `void SetOrderWithPosition(OrderType, WorldPosition)` | public | No |

**Chosen execution path:** resolve `OrderController` from `Mission.MainAgent.Team.GetOrderControllerOf(MainAgent) ?? Team.PlayerOrderController`, snapshot `SelectedFormations`, `ClearSelectedFormations`, `SelectFormation(target)`, issue `SetOrder` / `SetOrderWithPosition`, then restore prior selection in `finally` (best-effort; never throw from restore).

---

## `Formation` — direct movement orders (not used in Slice 12)

**Assembly:** `TaleWorlds.MountAndBlade`  
**Type:** `TaleWorlds.MountAndBlade.Formation`  

| Member | Signature | Access | Notes |
|--------|-----------|--------|-------|
| `SetMovementOrder` | `void SetMovementOrder(MovementOrder)` | public | Alternative to `OrderController`; would bypass native selection UX. |
| `Team` | (property used in mod code / engine) | public | Used in validation: formation must match `MainAgent.Team`. |

---

## `MovementOrder` — factories (documented, not chosen for Charge/Hold)

**Assembly:** `TaleWorlds.MountAndBlade`  
**Type:** `TaleWorlds.MountAndBlade.MovementOrder`  

Public **static** factories observed:

| Method | Returns | Parameters | Notes |
|--------|---------|------------|-------|
| `MovementOrderMove` | `MovementOrder` | `WorldPosition` | Matches “move to point” semantics; could pair with `Formation.SetMovementOrder`. |
| `MovementOrderChargeToTarget` | `MovementOrder` | `Formation` | Charges **toward** the given formation (typically an **enemy** formation), not a generic “charge” without target. |
| `MovementOrderFollow` | `MovementOrder` | `Agent` | Not used. |
| `MovementOrderFollowEntity` | `MovementOrder` | `GameEntity` | Not used. |
| `MovementOrderAttackEntity` | `MovementOrder` | `GameEntity`, `bool` | Not used. |

Nested enum `MovementOrder.MovementOrderEnum` includes values such as `Charge`, `ChargeToTarget`, `Move`, `Stop`, etc.

**Why we did not use `MovementOrder` for Charge / Hold:** public static helpers do not expose a clear “hold” factory, and “charge” without an enemy target maps cleanly to **`OrderType.Charge`** via **`OrderController.SetOrder`**, matching the native command bar.

---

## `WorldPosition` (for `Move` with position)

**Assembly:** `TaleWorlds.Engine`  
**Type:** `TaleWorlds.Engine.WorldPosition`  

| Member | Signature | Access |
|--------|-----------|--------|
| `.ctor` | `WorldPosition(Scene, Vec3)` | public |

Used with `OrderController.SetOrderWithPosition(OrderType.Move, worldPosition)`.

---

## `MissionOrder`

No type named `MissionOrder` was found as a public class in `TaleWorlds.MountAndBlade` on the inspected build (naming may differ or be internal elsewhere). **Slice 12 does not depend on it.**

---

## Summary

| Command (mod) | Native hook used |
|---------------|------------------|
| `Charge` | `OrderController.SetOrder(OrderType.Charge)` |
| `HoldPosition` | `OrderController.SetOrder(OrderType.StandYourGround)` |
| `MoveToPosition` | `OrderController.SetOrderWithPosition(OrderType.Move, new WorldPosition(scene, vec3))` |

- **Harmony:** **not required** for the above (all **public** APIs).  
- **Reflection in executor:** **none** (reflection remains isolated to Slice 11 terrain sampling, not order execution).
