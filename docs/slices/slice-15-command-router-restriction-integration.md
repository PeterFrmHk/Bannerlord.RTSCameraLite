# Slice 15 — Command router and formation restriction integration

## Purpose

Introduce a single validation path for player command intent **before** any native order execution. **`CommandRouter`** combines structural checks with **`FormationRestrictionService`**, which consults **commander mode**, **commander presence**, **doctrine profile**, **formation eligibility**, and **`CommanderConfig`** policy flags.

Native execution remains behind **`EnableNativePrimitiveOrderExecution`** (default **false**) and only routes through **`NativeOrderPrimitiveExecutor`**.

## Key types

| Type | Role |
|------|------|
| `CommandType` / `CommandIntent` | What was requested (formation, optional position/direction, requirement flags). |
| `CommandContext` | `Mission`, commander mode flag, `CommanderPresenceResult`, `FormationDoctrineProfile`, `FormationEligibilityResult`, trace reason. |
| `CommandValidationResult` | `Valid` / `Invalid` / `Blocked` (+ `Intent` reference). |
| `FormationRestrictionService` | `Evaluate` → `RestrictionDecision` (`Allow` / `Deny` / `Block`). |
| `CommandRouter` | `Validate` then `Decide` (optional primitive call when execution enabled). |
| `CommandExecutionDecision` | `ShouldExecute`, `RequiresNativeOrder`, `RequiresCavalrySequence`, `NativeOrderPrimitive`, `Reason`. |
| `NativeOrderPrimitive` | Maps to `NativeOrderPrimitiveExecutor` entry points only. |

## Debug keys (commander mode on)

| Key | Intent |
|-----|--------|
| **H** | `BasicHold` with formation center as `TargetPosition`. |
| **C** | `Charge`. |
| **M** | `AdvanceOrMove` with placeholder target (formation center + camera yaw offset when pose exists). |

Validation / decision lines are logged only when **`EnableCommandValidationDebug`** is true, throttled by **`CommandValidationDebugLogIntervalSeconds`**.

## Non-goals

- No full command UI.
- No native cavalry orchestration (sequence flag only).
- No forced per-troop movement from this slice.
- No bypass of `NativeOrderPrimitiveExecutor`.

## Audit (Slice 15)

- **A:** Command input can call native order execution directly.
- **¬A:** Commands must be filtered through commander presence, doctrine profile, eligibility, and restriction rules.
- **A\*:** `CommandRouter` validates command intent before any native order execution.
