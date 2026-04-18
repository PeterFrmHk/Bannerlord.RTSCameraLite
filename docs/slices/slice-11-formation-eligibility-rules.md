# Slice 11 — Formation eligibility rules

## Purpose

Define **which formation behaviors are allowed** for a player-team formation by combining **commander presence** (`CommanderPresenceResult`), **doctrine aggregates** (`FormationDoctrineProfile` from `DoctrineScoreCalculator`), and **equipment/composition** ratios (`FormationCompositionProfile`). This slice is **advisory only**: it does **not** issue native orders, block vanilla orders, move troops, or integrate a command router.

## Inputs

| Input | Role |
| --- | --- |
| `CommanderPresenceResult` | Whether a commander agent is recognized; certainty flags gate `AdvancedAdaptive`. |
| `FormationDoctrineProfile` | `DisciplineScore` (0..1) plus morale/training/equipment/rank proxies; embeds composition. |
| `FormationCompositionProfile` | Shield / polearm / mounted / cavalry-proxy / horse-archer ratios from agent equipment scans. |
| `FormationEligibilitySettings` | Thresholds sourced from `CommanderConfig` (JSON + defaults + harmonization). |

`DoctrineScoreCalculator` centralizes safe reads through `FormationDataAdapter.TryGetFormationAgents` and per-agent equipment inspection (wrapped in try/catch; **UNCERTAIN** when reads fail). Supporting types **`FormationDoctrineProfile`**, **`FormationCompositionProfile`**, and **`DoctrineScoreCalculator`** live under `src/Doctrine/` with the eligibility types.

`FormationEligibilityResult` exposes **`bool Success`** for evaluation outcome; the successful factory is **`CreateSuccess(...)`** (C# disallows a static method named `Success` on the same type).

## Allowed formation types

See `AllowedFormationType`: Mob through AdvancedAdaptive. Eligibility produces **allowed** and **denied** lists covering the full enum.

## Rule summary

- **No commander:** always **Mob**; **BasicHold** only if `NoCommanderAllowsBasicMobOrders` is true; all disciplined / advanced shapes denied (including ShieldWall, Square, Circle, Skein, mounted shapes, AdvancedAdaptive).
- **Commander present:** **BasicHold** + **BasicFollow**; **BasicLine** if `DisciplineScore` ≥ `BasicLineMinimumDiscipline`; **Loose** at `LooseMinimumDiscipline`; **ShieldWall** at shield-wall discipline **and** shield ratio; **Square** at square discipline **and** `PolearmRatio + ShieldRatio`; **Circle** + **Skein** at circle discipline; **MountedWide** at loose discipline **and** cavalry-proxy ratio; **HorseArcherLoose** at loose discipline **and** horse-archer ratio; **AdvancedAdaptive** only for **hero or captain** commander with high doctrine and **certain** commander + doctrine + composition data.

## CommanderMissionView

On the same **2.5s** cadence as the commander presence scan, after presence detection the view runs `DoctrineScoreCalculator.Compute` then `FormationEligibilityRules.Evaluate`. If `EnableEligibilityDebug` is true, it logs one compact line per formation, e.g. `Eligibility: Infantry allowed Mob, BasicHold, …; denied …`.

## Adapter boundaries

Equipment and agent probes stay inside `DoctrineScoreCalculator` / `FormationDataAdapter` patterns; eligibility rules remain pure policy on the computed profiles.

## Tests

`docs/tests/manual-test-checklist-slice-11.md`

## Audit

- **A:** “Commander presence alone should unlock formations.”
- **¬A:** “Formation discipline also depends on morale, training, equipment, rank, and troop composition.”
- **A\***: “Formation eligibility must combine commander presence with doctrine profile and equipment composition.”
