# Slice 12 — Commander rally absorption model

## Purpose

Introduce **rally and absorption state** plus **row / rank / spacing layout planning** so that future schedulers treat the **commander as the formation nucleus**. Only agents **inside `CommanderAbsorptionRadius`** receive **planned** `FormationSlotAssignment` data. This slice is **planning and telemetry only**: it does **not** issue native orders, force agent movement, patch native AI, or integrate the command router.

## Core rule

1. Troops are classified by planar distance to the **rally point** (commander position, else anchor, else formation center).
2. **Slot assignment** runs only for agents in the **inside absorption** band.
3. **Cooldown** (`SlotReassignmentCooldownSeconds`) limits how often a troop’s slot may change.

## Types and locations

| Area | Types |
| --- | --- |
| `src/Commander/` | `CommanderRallyPlanner`, `CommanderRallyState`, `CommanderAbsorptionZone`, `CommanderRallySettings` |
| `src/Doctrine/` | `TroopFormationState`, `TroopAbsorptionRecord`, `TroopAbsorptionController`, `FormationLayoutPlanner`, `RowRankSpacingPlan`, `FormationSlotAssignment`, `RowRankSlotAssigner` |
| Config | `CommanderConfig` / `commander_config.json` — radii, cooldown, scan interval, `EnableRallyAbsorptionDebug` |

## Spatial bands (`CommanderAbsorptionZone`)

Planar XY distance from rally point:

- **Inside absorption** — `d ≤ CommanderAbsorptionRadius`
- **Rallying** — `CommanderAbsorptionRadius < d ≤ CommanderRallyRadius`
- **Outside rally, within cohesion** — `CommanderRallyRadius < d ≤ CohesionBreakRadius`
- **Outside cohesion** — `d > CohesionBreakRadius`

`CommanderRallySettings.FromConfig` clamps **absorption ≤ rally** and **cohesion ≥ rally + 1**.

## Mission integration (`CommanderMissionView`)

On **`RallyScanIntervalSeconds`** (not every frame), for each player-team formation with units: detect commander → resolve anchor → compute doctrine → eligibility → `FormationCompositionAnalyzer.Analyze` → `FormationLayoutPlanner.Build` → `TroopAbsorptionController.SyncFormation` → `CommanderRallyPlanner.BuildRallyState` for logging.

If **`EnableRallyAbsorptionDebug`** is true, one line per formation, for example:  
`Rally absorption: Infantry 32 total, 18 absorbable, 12 assigned.`

## Non-goals (this slice)

- No native move / charge / hold orders.
- No forced agent positions.
- No command router integration.
- No cavalry release / reform.
- No visual slot rendering.

## Tests

`docs/tests/manual-test-checklist-slice-12.md`

## Audit

- **A:** “A scheduler can assign all troops directly into formation slots.”
- **¬A:** “Direct assignment from far away causes pathing chaos and unrealistic formation assembly.”
- **A\*:** “Troops rally to the commander first, then are absorbed into row / rank / spacing slots only when close enough.”

**Note:** The repository file `docs/slice-12-audit.md` describes a **different** (future) slice theme (native order execution) under an older numbering scheme; this **Slice 12** deliverable is the **rally absorption model** above.
