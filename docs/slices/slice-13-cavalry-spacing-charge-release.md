# Slice 13 — Cavalry spacing and charge-release doctrine

## Purpose

Add **cavalry-specific spacing** and a **charge / position-lock / reform** doctrine model so planners can treat mounted formations differently from infantry. This slice is **planning and telemetry only**: it does **not** issue native orders, force movement, or call charge / advance / hold APIs.

## Core rules (implemented as data + policies)

1. **Cavalry-heavy** formations receive **wider** lateral and depth spacing than infantry baselines.
2. **Horse-archer-heavy** formations receive the **widest** spacing and do **not** use shock-cavalry “release lock after close contact” behavior.
3. **Position lock** is **advisory-allowed** during rally / assembly / charge-ready phases (`CavalryPositionLockPolicy` + `RowRankSpacingPlan.PositionLockAllowed`).
4. Lock **release** is advised when distance to the primary enemy formation center is within **`CavalryReleaseLockDistance`** or **`CavalryImpactDetector`** reports close contact / impact heuristics.
5. Lock **re-activation** is gated by **`CavalryPositionLockPolicy.ShouldReactivatePositionLock`** (reform distance, cooldown, not still inside release radius, disciplined reform allowed).
6. **Disciplined reform** requires a **valid commander** unless **`AllowCavalryReformWithoutCommander`** is true (`CavalryReformPolicy`).

## Types (`src/Doctrine/`)

| Type | Role |
| --- | --- |
| `CavalryChargeState` | Enum lifecycle states (mounted assembly through reform / morale / commander loss). |
| `CavalryChargeSequenceState` | Per-formation snapshot: target, distances, lock/reform flags, reason string. |
| `CavalrySpacingRules` | Composition-based mounted / HA detection and spacing multipliers (training / morale widen). |
| `CavalryPositionLockPolicy` | `ShouldAllowPositionLock` / `ShouldReleasePositionLock` / `ShouldReactivatePositionLock`. |
| `CavalryImpactDetector` | Distance-first contact; optional mounted speed-drop sample; capped enemy-agent proximity scan via `FormationDataAdapter`. |
| `CavalryReformPolicy` | `TryEvaluateReformDisciplineAllowed` — commander, lock history, distances, cooldown, enemy spacing fallback. |
| `CavalryDoctrineRules` | `Evaluate(...)` — enemy target pick, metrics, state classification. |

## `RowRankSpacingPlan` (Slice 13 fields)

- `IsMountedLayout`, `IsHorseArcherLayout`
- `PositionLockAllowed`, `ReleaseLockAfterCloseContact`
- `ReformDistance` (from `CommanderConfig.CavalryReformDistanceFromAttackedFormation`)
- `MountedDoctrineReason`

## `FormationLayoutPlanner`

When composition is cavalry-heavy, **`CavalrySpacingRules.ApplyMountedSpacing`** overrides lateral/depth and sets mounted flags. Infantry and non-mounted paths keep the Slice 12 heuristic.

## `CommanderMissionView`

On **`DoctrineScanIntervalSeconds`**, for **player cavalry-heavy** formations: compute doctrine, eligibility, layout plan, rally state, then **`CavalryDoctrineRules.Evaluate`**. Logs when **`EnableCavalryDoctrineDebug`**: state changes, throttled “wide spacing”, lock release edge, reform-ready edge.

## Config (`CommanderConfig` / JSON)

Spacing: `CavalryLateralSpacing`, `CavalryDepthSpacing`, `HorseArcherLateralSpacing`, `HorseArcherDepthSpacing`.  
Lock / reform: `CavalryReleaseLockDistance`, `CavalryReformDistanceFromAttackedFormation`, `CavalryReformCooldownSeconds`, `CavalryMinimumEnemyDistanceToReform`.  
Impact heuristics: `CavalryImpactEnemyDistance`, `CavalryImpactSpeedDropThreshold`, `CavalryImpactAgentRatio`.  
Debug: `EnableCavalryDoctrineDebug`. Fallback reform: `AllowCavalryReformWithoutCommander` (default false).

## Tests

`docs/tests/manual-test-checklist-slice-13.md`

## Audit

- **A:** “Cavalry can use the same position-lock logic as infantry.”
- **¬A:** “Cavalry depends on spacing, charge impact, release, disengagement, and reform distance.”
- **A\*:** “Cavalry needs a separate charge-release doctrine: wide spacing before charge, lock release near contact, and reform only after distance / cooldown under commander control.”
