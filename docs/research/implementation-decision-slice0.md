# Implementation decision — Slice 0 (research gate)

**Scope:** Slice **0** is **research-only**. This document records **go / no-go** decisions for later engineering slices, grounded in **local installed** Bannerlord **v1.3.15** (Native `SubModule.xml`) assemblies under:

- `...\Mount & Blade II Bannerlord\bin\Win64_Shipping_Client\`
- `...\Mount & Blade II Bannerlord\Modules\...\` (incl. `Native\bin\Win64_Shipping_Client\` for `TaleWorlds.MountAndBlade.View.dll`)
- `...\steamapps\workshop\content\261550\` (installed mods — **patterns only**)

**Related research:** [`base-game-camera-scan.md`](base-game-camera-scan.md), [`base-game-order-scan.md`](base-game-order-scan.md), [`base-game-formation-layout-scan.md`](base-game-formation-layout-scan.md), [`installed-mod-reference-scan.md`](installed-mod-reference-scan.md), [`native-cavalry-command-sequence.md`](native-cavalry-command-sequence.md).

---

## 1. Verdict summary

| Area | Decision | Status |
| --- | --- | --- |
| RTS camera integration | **GO** on public `MissionView.UpdateOverridenCamera` + `MissionScreen.CombatCamera.Frame` / `Mission.SetCameraFrame` fallback | **GO** (with listed UNCERTAINs) |
| Native order issuance | **GO** on public `OrderController` + `WorldPosition` | **GO** |
| Formation layout reads | **GO** on `Formation` + `IFormationArrangement` | **GO** |
| `OrderPositionLock` writes | **NO-GO** for typed implementation | **BLOCKER** until ILSpy |
| Cavalry sequence automation | **NO-GO** for “release lock” step | **BLOCKER** (ties to lock type) |
| Harmony | **Avoid now**; **defer** only if public hooks disappear | **AVOID** |

---

## 2. Battle mission lifecycle (read-only signals)

**Decision:** Use **`Mission`** public members for phase gating: `Initialize` / `AfterStart` / `OnTick` pipeline (via `MissionBehavior` / `MissionView` hooks), **`get_MissionEnded`**, **`get_MissionIsEnding`**, **`get_IsFinalized`**, **`EndMission`**, and mission-state callbacks such as **`OnMissionStateActivate` / `OnMissionStateDeactivate` / `OnMissionStateFinalize`** (names observed via reflection on `TaleWorlds.MountAndBlade.dll`).

**UNCERTAIN:** exact ordering relative to camera teardown vs. order UI — validate with logging in a later slice.

---

## 3. Safest camera path

**Decision:** **Primary:** `MissionView.UpdateOverridenCamera(float)` returns `true` while RTS owns the frame; apply pose via **`MissionScreen.CombatCamera`** → `TaleWorlds.Engine.Camera.Frame` (`MatrixFrame`). **Fallback:** `Mission.SetCameraFrame` / `GetCameraFrame` + `ResetFirstThirdPersonView` for restore experiments.

**Rationale:** All members are on **supported public** surfaces in local `TaleWorlds.MountAndBlade.View.dll` / `TaleWorlds.MountAndBlade.dll` / `TaleWorlds.Engine.dll` (see camera scan).

**UNCERTAIN / follow-up:** `MissionScreen.UpdateFreeCamera`, `SetCameraLockState`, and several **restore** method names may be **build-specific** — re-verify with ILSpy before hard dependency.

---

## 4. Safest native order path

**Decision:** **`OrderController`** on the player team (`Team.PlayerOrderController` / `Team.GetOrderControllerOf(Agent)`), after explicitly managing **`SelectedFormations`**, using:

- `SetOrderWithPosition(OrderType.Move, WorldPosition)` for **move / positional** tasks  
- `SetOrder(OrderType.Charge)` / `ChargeWithTarget` for **charge**  
- `SetOrder(OrderType.StandYourGround)` for **hold**  
- `SetOrder(OrderType.Advance)` (or `AdvanceTenPaces`) for **forward** flavor when appropriate

**Direct** `Formation.SetMovementOrder(MovementOrder.*)` remains a **secondary** path for **move** when selection mutation is undesirable.

**Rationale:** Highest parity with native UI order pipeline; public API.

**UNCERTAIN:** selection restoration must mirror vanilla expectations (deployment vs battle).

---

## 5. Safest formation data path

**Decision:** Read **geometry** from `Formation.Arrangement` (`IFormationArrangement`: `RankCount`, `RankDepth`, `Width`, `Depth`, per-unit world positions) and **anchors** from `OrderPosition` / `OrderGroundPosition` guarded by `OrderPositionIsValid`. Use **`FacingOrder` / `ArrangementOrder`** facets for facing & shape.

**Rationale:** Avoids guessing internal unit arrays; uses the same abstraction the engine uses for layout.

**BLOCKER:** anything depending on **`OrderPositionLock`** semantics stays **blocked** until the true type and lifecycle are known (reflection reported `System.Object` — likely metadata/tooling oddity, but **must not** ship guesses).

---

## 6. Commander / captain / hero detection

**Decision:** **GO (read-only)** using public **`Team`** and **`Agent`** surfaces observed on `TaleWorlds.MountAndBlade.dll`:

| Signal | API (pattern) |
| --- | --- |
| Player’s hero / character | `Agent.IsHero`, `Agent.Character`, `Mission.MainAgent` |
| Player troop vs general | `Agent.IsPlayerTroop`, `Agent.IsPlayerControlled`, `Team.IsPlayerGeneral`, `Team.IsPlayerSergeant` |
| Command post / general stack | `Team.GeneralAgent`, `Team.GeneralsFormation`, `Team.Leader`, `Team.BodyGuardFormation` |
| Which formation an agent is in | `Agent.Formation` |
| Remote leadership capability | `Agent.CanLeadFormationsRemotely` |

**UNCERTAIN:** which of `Leader` vs `GeneralAgent` best matches product language “commander” in all mission types (field battle vs siege vs OOB). **BLOCKER:** none for **reads** — only gameplay policy must choose precedence.

---

## 7. Backspace conflict decision

**Facts from local install:**

- `TaleWorlds.InputSystem.InputKey` includes **`BackSpace`** (enum member present).  
- `TaleWorlds.MountAndBlade.View.dll` exports `MissionOrderUIHandler` and multiple **order UI / visual order** types.  
- Native string tables include **mission order hotkey** ids such as `MissionOrderHotkeyCategory_ViewOrders` (`Modules\Native\ModuleData\global_strings.xml`).

**Decision:**

1. **Do not** bind mod RTS actions to **`InputKey.BackSpace`** in default `rts_camera_lite.json` — treat **Backspace as reserved** for native order UI unless proven otherwise on a pinned build.  
2. Prefer **configurable** keys and document the conflict in the manual test checklist.  
3. **UNCERTAIN:** the exact vanilla default mapping for “toggle order view” vs **BackSpace** is stored in **player/engine hotkey tables**, not located in a simple static XML grep on this pass — confirm in-game on a clean profile.

**No Harmony** is required merely to *avoid* the conflict; input routing discipline is sufficient **if** the mod does not consume Backspace while orders should reach vanilla.

---

## 8. Cavalry sequence decision

**Decision:** **Partial GO** — the **order primitives** (`Advance` / `Move` / `Charge` / `StandYourGround` / arrangement orders) are present on **`OrderType`** and **`OrderController`**. **NO-GO** for automating **“release position lock near contact”** until `OrderPositionLock` is understood.

**Plan:** implement distance-gated **Move → Charge** using **`CachedClosestEnemyFormationDistanceSquared`** only after validating cache freshness (**UNCERTAIN**). **Reform after 30 m** is a **mod-local policy** on top of `StandYourGround` + arrangement/facing resets — not a single discovered `OrderType.Reform`.

---

## 9. Harmony — now / later / avoid

| Horizon | Stance |
| --- | --- |
| **Now (Slice 0–12 baseline)** | **Avoid.** Public `MissionView`, `Mission`, `OrderController`, `Formation` APIs cover RTS camera + orders. |
| **Later** | **Defer** minimal Harmony only if TaleWorlds **removes or seals** a required public hook (e.g. camera override entry) **and** no supported alternative exists on pinned versions. |
| **Never (aspirational)** | Avoid Harmony for **core** camera/order paths to reduce patch conflict surface (see workshop scan). |

---

## 10. Later slices blocked until APIs are verified

| Slice / theme | Blocked until |
| --- | --- |
| **Cavalry doctrine (release lock + reform fidelity)** | ILSpy + runtime validation of **`OrderPositionLock`**; distance cache semantics |
| **Camera restore polish** | Confirmed `MissionScreen` / base-class **restore** methods on **1.3.15** View DLL |
| **Commander / captain UX anchors** | Mission tests mapping `Team.GeneralAgent`, `Team.Leader`, `IsPlayerGeneral`, `IsPlayerSergeant`, `Agent.Formation` to player intent |
| **Tight Gauntlet / order bar co-existence** | Optional: read-only analysis of `MissionOrderUIHandler` — **not required** if mod stays overlay-light |

---

## 11. Sign-off

Slice **0** documentation set is **complete enough to start** Slice 1+ engineering on **camera + orders + formation reads**, with explicit **BLOCKERs** where the engine surface is opaque. Re-run ILSpy on any TaleWorlds update before widening scope to **position lock** or **Gauntlet-level** integration.
