# Scenario: custom battle — cavalry-heavy

## Goal

Validate **cavalry doctrine** planning row in diagnostics and optional **native cavalry charge sequence** when explicitly enabled in config.

## Setup

1. **Custom battle** — player includes **one or more cavalry-heavy** formations (shock or horse archer mix per mod rules).
2. Note `EnableNativeCavalryChargeSequence` and related keys in `commander_config.json` before testing.

## Steps

1. Deploy; enable **commander mode**.
2. Select **cavalry** formation; press **F9** → diagnostics **ON**.
3. Observe **`Cav`** segment:
   - Planning path: prefix **`Pl:`** + `CavalryChargeState` token.
   - If native sequence active and not aborted: may show **`Nv:`** + state (config-dependent).
4. If testing **native sequence** (debug key **N** where implemented): confirm orchestrator starts only when config + validation allow; no soft-lock on abort.

## Pass criteria

- Diagnostics refresh throttled (~1s default); no spam freeze.
- Cavalry line differs measurably from infantry-only scenario (`custom-battle-infantry.md`).
- With native execution **disabled**, no unexpected charge orders; `[Diag]` **`Native ex=false`** consistent with config.

## Evidence

- Screenshot: `[Diag]` for cavalry group.
- Short clip: select cav → `[Diag]` updates across 2–3 refresh intervals.
