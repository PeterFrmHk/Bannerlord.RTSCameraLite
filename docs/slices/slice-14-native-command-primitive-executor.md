# Slice 14 — Native command primitive executor

## Purpose

Provide **`NativeOrderPrimitiveExecutor`** as the **only** adapter that calls TaleWorlds **`OrderController`** primitives for player-team formations. Callers receive **`NativeOrderResult`** (`Executed`, `NotWired`, `Blocked`, `Primitive`, `Message`) and the executor **never throws** to consumers.

## Slice 0 research references

- [`docs/research/base-game-order-scan.md`](../research/base-game-order-scan.md) — public `OrderController` + selection workflow + `WorldPosition` / `OrderType` mapping.
- [`docs/research/native-cavalry-command-sequence.md`](../research/native-cavalry-command-sequence.md) — cavalry sequence design notes (orchestration is Slice 16; this slice is primitives only).
- [`docs/research/implementation-decision-slice0.md`](../research/implementation-decision-slice0.md) — **GO** on public `OrderController` + `WorldPosition`; **NO-GO** on typed `OrderPositionLock` automation; **AVOID** Harmony unless research explicitly approves.

## Native API path — **WIRED** (public APIs)

Pinned reference assemblies in the project: **1.2.12.66233** (`Bannerlord.RTSCameraLite.csproj`).

| Primitive | Researched API | Signature (conceptual) |
| --- | --- | --- |
| AdvanceOrMove / Reform anchor | `OrderController.SetOrderWithPosition(OrderType.Move, WorldPosition)` | `void SetOrderWithPosition(OrderType orderType, WorldPosition orderPosition)` |
| Charge (no target formation) | `OrderController.SetOrder(OrderType.Charge)` | `void SetOrder(OrderType orderType)` |
| Charge (with target formation) | `OrderController.SetOrderWithFormation(OrderType.ChargeWithTarget, Formation)` | `void SetOrderWithFormation(OrderType orderType, Formation formation)` |
| Hold / Stop | `OrderController.SetOrder(OrderType.StandYourGround)` | `void SetOrder(OrderType orderType)` |
| Follow commander | `OrderController.SetOrderWithAgent(OrderType.FollowMe, Agent)` | `void SetOrderWithAgent(OrderType orderType, Agent agent)` |

Selection: **`ClearSelectedFormations`**, **`SelectFormation`**, **`IsFormationSelectable`**, restore prior selection in `OrderSelectionScope` (`IDisposable`).

**Stop:** No `OrderType.Stop` on the pinned surface in research; **Stop** maps to **`StandYourGround`** (halt advance), documented in research notes.

## Config gates (`CommanderConfig`)

| Flag | Default | Role |
| --- | --- | --- |
| `EnableNativeOrderExecution` | **false** | Master switch; when false, all primitives return **`BlockedResult`**. |
| `AllowNativeAdvanceOrMove` | true | Per-primitive allow. |
| `AllowNativeCharge` | true | Per-primitive allow. |
| `AllowNativeHold` | true | Per-primitive allow. |
| `AllowNativeReform` | true | Per-primitive allow. |
| `AllowNativeFollowCommander` | true | Per-primitive allow. |
| `AllowNativeStop` | true | Per-primitive allow. |
| `EnableNativeOrderDebug` | true | Throttled / once-style debug (disabled execution uses `LogWarningOnce`). |

`EnableNativePrimitiveOrderExecution` (Slice 15) and **`EnableNativeOrderExecution`** must both be true for **`CommandRouter`** to invoke primitives.

## Safety behavior

- **Null** `NativeOrderExecutionContext`, **null** mission, **null** source formation → **`Failure`** (or primitive-specific precondition failure).
- **AdvanceOrMove** / **Reform** without finite **`TargetPosition`** → **`Failure`** before engine call.
- **`Mission.MissionEnded`** → **`BlockedResult`**.
- **`EnableNativeOrderExecution`** false or per-primitive allow false → **`BlockedResult`** (master gate logs once when debug enabled).
- **`FormationEligibilityResult`** present and not **`Success`** → **`BlockedResult`** (“formation eligibility did not approve this behavior”). **Null eligibility** does not block (used by doctrine internal contexts until router supplies eligibility).
- Source formation must belong to **`Mission.PlayerTeam`**.
- **Invalid / null target formation** for charge: charge falls back to **`OrderType.Charge`** without throwing.
- **FollowCommander:** requires a follow agent (`FollowTargetAgent` or `Mission.MainAgent`); missing agent → caught → **`Failure`**.
- **Try/catch** around the whole **`Execute`** path → **`Failure`** with message, no outward throw.

## Blocked / not-wired behavior

- **Blocked:** config master switch off, mission ended, eligibility failed, formation not selectable, not player-owned formation, per-primitive disabled.
- **NotWired:** reserved for a future **NO-GO** research outcome; Slice 0 approved **`OrderController`**, so the live build uses **`Success`** / **`Failure`** / **`BlockedResult`** only. If research later revokes the path, return **`NotWiredResult`** from this class only (no scattered calls).

## Harmony / reflection

**Not used.** Slice 0 says **AVOID** Harmony unless public hooks disappear. No reflection helpers.

## Tests

- Automated: none required for this slice (Bannerlord host).
- Manual: [`docs/tests/manual-test-checklist-slice-14.md`](../tests/manual-test-checklist-slice-14.md).

## Audit

| Claim | Negation | Resolution |
| --- | --- | --- |
| **A:** Doctrine systems should call native order APIs directly. | **¬A:** Version-sensitive and risky if scattered. | **A\*:** All primitives go through **`NativeOrderPrimitiveExecutor`** with typed context and safe results. |

## Non-goals (unchanged)

- No cavalry orchestration logic in this slice (orchestrator remains Slice 16).
- No command-router redesign beyond using **`NativeOrderExecutionContext`** + master gate.
- No UI, no per-agent puppeteering, no `OrderPositionLock` automation.
