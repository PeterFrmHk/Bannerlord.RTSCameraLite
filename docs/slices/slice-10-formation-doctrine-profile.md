# Slice 10 — Formation doctrine profile (data only)

## Purpose

Build a **doctrine scoring profile** for each friendly formation before any formation eligibility, restriction, or native ordering uses that data. This slice computes **read-only** aggregates: morale, training, equipment, commander quality, cohesion, casualty shock, rank quality, and a weighted **formation discipline** score.

## Components

| Area | Type | Role |
|------|------|------|
| `FormationDoctrineProfile` | Doctrine | Holds clamped 0..1 scores plus `FormationCompositionProfile`, `Reason`, `IsCertain`. |
| `DoctrineScoreCalculator` | Doctrine | `Compute(Mission, Formation, CommanderPresenceResult, DoctrineScoreSettings)` → `DoctrineScoreResult`. |
| `DoctrineScoreSettings` / `DoctrineScoreResult` | Doctrine | Config-driven weights; `Success` / `Failure` / `Uncertain` factories (`DoctrineScoreResult` uses `ComputationSucceeded` to avoid clashing with the static `Success` factory in C#). |
| `FormationCompositionAnalyzer` | Equipment | Agent-level role mix via `FormationDataAdapter` + classifiers; safe on empty formations. |
| `EquipmentRoleClassifier` / `TroopRankClassifier` | Equipment | Defensive reads; unknown / fallback without throwing. |
| `FormationDataAdapter` | Adapters | `TryGetAgentEquipmentHints`, `TryGetAgentHealthRatio`, `TryGetAgentMorale01` (stub until a stable morale API exists). |

## Mission integration

`CommanderMissionView` runs a doctrine pass on a **`DoctrineScanIntervalSeconds`** cadence (default **3s**), separate from the commander presence interval. When **`EnableDoctrineDebug`** is true, it logs one throttled summary line, for example:

`Doctrine scan: Cavalry discipline 0.71, Infantry discipline 0.62.`

## Non-goals (unchanged)

- No formation access restrictions.
- No native orders.
- No forced commander or troop movement.

## Audit (Slice 10)

- **A:** Formation eligibility can be decided directly from commander presence.
- **¬A:** True formation discipline depends on morale, training, equipment, rank, cohesion, casualty shock, and commander quality.
- **A\*:** Build a doctrine scoring profile before applying formation eligibility or restrictions.
