# Slice 16 — Native cavalry command sequence orchestrator

## Goal

Drive shock cavalry doctrine using **native** order primitives (`NativeOrderPrimitiveExecutor`) behind a small state machine, instead of custom per-agent movement.

## Doctrine (summary)

1. **Assemble** — wide spacing context (`CavalrySpacingRules` / planning elsewhere); position lock allowed.
2. **Advance / move** — while farther than `CavalryForwardToChargeDistance`, issue `ExecuteAdvanceOrMove` toward the resolved target (throttled re-issue every 4s while still out of range).
3. **Charge** — at or inside forward distance, issue `ExecuteCharge` when `CavalryUseNativeChargeCommand` is true.
4. **Release lock** — at `CavalryReleaseLockDistance` or when `CavalryImpactDetector` reports close/impact contact; do not force slot lock through close combat.
5. **Disengage** — allow native charge behavior; track distance from target formation or recorded impact point.
6. **Reform** — only when `CavalryReformPolicy` allows (including ≥ `CavalryReformDistanceFromAttackedFormation` default 30m, cooldown, commander validity, nearest-enemy safety when known); then `ExecuteHoldOrReform` at a reform position and re-enable lock semantics.

## Types (src/Doctrine)

| Type | Role |
|------|------|
| `CavalryNativeChargeOrchestrator` | `StartSequence`, `TickSequence`, `AbortSequence` |
| `CavalrySequenceRegistry` | One active sequence per source formation; cleanup of dead formations |
| `CavalryTargetTracker` | Target formation/position resolution and distance helpers (safe fallbacks) |
| `CavalrySequenceTickResult` | `Continued`, `Completed`, `Aborted`, `NewState`, `Message`, `NativeOrderResult` |

`CavalryChargeSequenceState` is extended with mutable native-sequence fields (forward/charge issued, timers, abort flags, last known target).

## Integration

- **`CommandRouter`**: `NativeCavalryChargeSequence` does **not** map to a direct primitive in `Decide`; `TryStartNativeCavalryChargeSequence` validates config + wiring probe then calls the orchestrator.
- **`CommanderMissionView`**: Throttled tick (~0.5s) runs active sequences; passive `CavalryDoctrineRules` scan is **skipped** for formations with an active native sequence to avoid conflicting doctrine updates. Debug key **N** builds a nearest-enemy intent and attempts a start.
- **`NativeOrderExecutionContext`**: Optional tag on executor calls (probe vs tick).

## Config (`CommanderConfig`)

| Key | Default | Notes |
|-----|---------|--------|
| `EnableNativeCavalryChargeSequence` | `false` | Master switch for orchestrator |
| `EnableNativePrimitiveOrderExecution` | `false` | Must be true for start or tick to use executor |
| `CavalryUseNativeForwardBeforeCharge` | `true` | |
| `CavalryUseNativeChargeCommand` | `true` | |
| `CavalryForwardToChargeDistance` | `25` | Meters |
| `CavalryReleaseLockDistance` | `12` | Slice 13 (unchanged) |
| `CavalryReformDistanceFromAttackedFormation` | `30` | Slice 13 |
| `CavalryReformCooldownSeconds` | `6` | Slice 13 |
| `CavalryMinimumEnemyDistanceToReform` | `20` | Slice 13 |
| `EnableCavalrySequenceDebug` | `true` | Gates the five transition log lines |

Legacy JSON without Slice 16 keys gets defaults via `CommanderConfigService.ApplyOmittedSlice16NativeCavalrySequenceDefaults`.

## Safety

Orchestrator and registry swallow exceptions; sequences abort on invalid commander, dead/empty formation, lost target with no fallback, or `NativeOrderResult.NotWired` mid-sequence. Infantry routing is unchanged.

## Audit (Slice 16)

- **A:** Cavalry doctrine could directly control cavalry through custom movement.
- **¬A:** Bannerlord already exposes native advance/move/charge/hold primitives that should be reused where possible.
- **A\*:** Cavalry doctrine should orchestrate native primitives through a state machine, release custom lock near impact, and reform only after disengagement (≥30m + policy gates).

## Non-goals

Full command UI, tactical markers, per-agent steering, horse-archer skirmish doctrine in this slice.
